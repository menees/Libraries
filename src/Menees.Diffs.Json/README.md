# Menees.Diffs.Json

This library generates [RFC 6902](https://www.rfc-editor.org/rfc/rfc6902) JSON Patch documents
by comparing two JSON values using the Myers diff algorithm from
[Menees.Diffs](../Menees.Diffs/README.md).

## Target Frameworks

| Target | Notes |
|--------|-------|
| `netstandard2.0` | Compatible with .NET Framework 4.6.1+, .NET Core 2.0+, and Unity |
| `net10.0` | .NET 10 |
| `net48` | .NET Framework 4.8 |

`System.Text.Json` is included inbox on `net10.0`; it is referenced as a NuGet package on the
older targets.

## How It Works

1. **Object diffing** — Properties are compared by name (using a sorted union of both sides).
   Added properties produce `add` operations; removed properties produce `remove` operations;
   properties present in both sides are recursed into.

2. **Array diffing** — Each element is serialized to a *canonical* JSON string (objects with
   properties sorted by name at every level) and the resulting string arrays are fed to
   `MyersDiff<string>`. The Myers algorithm finds the longest common subsequence, yielding
   the minimum number of `add`/`remove`/`replace` operations.  Because the edit script is
   walked forward with an *insertion offset* counter, all emitted array indices remain
   correct after each prior operation shifts the live array.  `Change` edit pairs (one
   delete + one insert at the same position) are recursed into rather than emitted as a
   coarse `replace`, so the patch drills down to the leaf that actually changed.

   > **Limitation — reordered + mutated arrays:** Element matching is purely positional
   > within the longest common subsequence.  When an array is *simultaneously* reordered
   > and mutated (e.g. `[A, B]` → `[B', A']` where both elements also changed), Myers has
   > no exact-match anchors and pairs elements by position, producing a valid but
   > potentially coarser patch than one that tracks element identity.  If elements carry a
   > stable key property (e.g. `"id"`), callers can pre-sort both arrays by that key before
   > calling `CreatePatch` to get more semantically accurate results.

3. **Scalar / kind-mismatch diffing** — Values of different kinds (e.g. a string replaced
   by a number), and scalars whose canonical serializations differ, produce a `replace`
   operation at that pointer.

All paths use **JSON Pointer** (RFC 6901) notation:
- `/` separates path segments.
- `~` is escaped as `~0`; `/` within a segment is escaped as `~1`.
- Array indices are zero-based integers (e.g. `/items/0`).

## API Reference

### `JsonDiff` (static class)

```csharp
// Returns a JsonArray of RFC 6902 operation objects.
JsonArray CreatePatch(JsonElement original, JsonElement modified);
JsonArray CreatePatch(string originalJson, string modifiedJson);

// Returns the patch serialized as a JSON string.
string CreatePatchText(JsonElement original, JsonElement modified);
string CreatePatchText(string originalJson, string modifiedJson);
```

The `string` overloads parse the input using `JsonDocument.Parse`.

The returned `JsonArray` contains `JsonObject` nodes, each with:
- `"op"` — one of `"add"`, `"remove"`, `"replace"`
- `"path"` — a JSON Pointer string
- `"value"` — the new value (present for `"add"` and `"replace"`; absent for `"remove"`)

An empty array means the two documents are identical.

---

## Examples

### (a) Scalar Replace

```csharp
string original = """{"version": 1}""";
string modified = """{"version": 2}""";

string patch = JsonDiff.CreatePatchText(original, modified);
// [{"op":"replace","path":"/version","value":2}]
```

---

### (b) Object — Add, Remove, and Replace Properties

```csharp
string original = """{"name":"Alice","role":"user","score":10}""";
string modified = """{"name":"Alice","role":"admin","level":3}""";

JsonArray patch = JsonDiff.CreatePatch(original, modified);
// [
//   {"op":"remove","path":"/score"},
//   {"op":"replace","path":"/role","value":"admin"},
//   {"op":"add","path":"/level","value":3}
// ]
```

Properties are processed in ordinal-sorted name order, so the output is deterministic
regardless of how the properties are ordered in the source JSON.

---

### (c) Nested Object — Recursion to the Changed Leaf

```csharp
string original = """
	{
		"user": { "id": 1, "address": { "city": "London", "zip": "EC1A" } }
	}""";

string modified = """
	{
		"user": { "id": 1, "address": { "city": "Paris", "zip": "75001" } }
	}""";

string patch = JsonDiff.CreatePatchText(original, modified);
// [
//   {"op":"replace","path":"/user/address/city","value":"Paris"},
//   {"op":"replace","path":"/user/address/zip","value":"75001"}
// ]
```

The library recurses into nested objects rather than emitting a single coarse `replace` for
`/user/address`, so callers get the minimum set of fine-grained operations.

---

### (d) Array — Myers Diff for Insert and Delete

```csharp
string original = """{"tags":["red","blue","green"]}""";
string modified = """{"tags":["red","purple","green","yellow"]}""";

string patch = JsonDiff.CreatePatchText(original, modified);
// [
//   {"op":"replace","path":"/tags/1","value":"purple"},
//   {"op":"add","path":"/tags/3","value":"yellow"}
// ]
```

`"red"` and `"green"` are in the longest common subsequence; only `"blue"` → `"purple"`
(a Change at index 1) and the new `"yellow"` at the end (an Insert) are emitted.

For a pure insertion at the start of an array:

```csharp
string original = """{"items":["a","b","c"]}""";
string modified = """{"items":["x","a","b","c"]}""";

string patch = JsonDiff.CreatePatchText(original, modified);
// [{"op":"add","path":"/items/0","value":"x"}]
```

One operation, not four — because the insertion offset tracking keeps all subsequent indices
correct.

---

### (e) ASP.NET Core `JsonPatchDocument<T>` Interop

The `Menees.Diffs.Json` library has no dependency on `Microsoft.AspNetCore.JsonPatch.SystemTextJson`.
However, a `JsonArray` can be round-tripped into a `JsonPatchDocument<T>` via
`JsonSerializer.Deserialize`:

```csharp
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using System.Text.Json;

// Generate the patch.
JsonArray patch = JsonDiff.CreatePatch(originalJson, modifiedJson);

// Deserialize into a strongly-typed patch document.
JsonPatchDocument<MyModel> patchDoc =
	JsonSerializer.Deserialize<JsonPatchDocument<MyModel>>(patch.ToJsonString())!;

// Apply to a target object.
MyModel target = GetCurrentModel();
patchDoc.ApplyTo(target);
```

Alternatively, keep it untyped:

```csharp
JsonPatchDocument patchDoc =
	JsonSerializer.Deserialize<JsonPatchDocument>(patch.ToJsonString())!;
patchDoc.ApplyTo(target);
```

---

### (f) JSON Pointer Escaping

Property names that contain `~` or `/` are automatically escaped per RFC 6901:

```csharp
string original = """{"a/b":1,"c~d":2}""";
string modified = """{"a/b":9,"c~d":2}""";

string patch = JsonDiff.CreatePatchText(original, modified);
// [{"op":"replace","path":"/a~1b","value":9}]
//                         ^^^^ '/' escaped as '~1'
```
