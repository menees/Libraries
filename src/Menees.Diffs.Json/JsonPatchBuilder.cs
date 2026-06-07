namespace Menees.Diffs.Json;

#region Using Directives

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

#endregion

/// <summary>
/// Builds a list of RFC 6902 JSON Patch operations by recursively diffing two
/// <see cref="JsonElement"/> trees using the following rules:
/// <list type="bullet">
///   <item>Objects — diff by sorted property name (set arithmetic: add/remove/recurse).</item>
///   <item>Arrays — run <see cref="MyersDiff{T}"/> on canonical element strings,
///     walk edits forward with an insertion-offset counter, recurse into Change pairs.</item>
///   <item>Scalars / kind mismatch — emit replace if values differ.</item>
/// </list>
/// </summary>
internal sealed class JsonPatchBuilder
{
	#region Private Data Members

	private readonly List<JsonObject> operations = [];

	#endregion

	#region Public Properties

	public IReadOnlyList<JsonObject> Operations => this.operations;

	#endregion

	#region Public Methods

	/// <summary>
	/// Recursively diffs <paramref name="original"/> against <paramref name="modified"/>,
	/// appending RFC 6902 operations to <see cref="Operations"/>.
	/// <paramref name="pointer"/> is the RFC 6901 JSON Pointer to the current node.
	/// </summary>
	public void Diff(JsonElement original, JsonElement modified, string pointer)
	{
		if (original.ValueKind == JsonValueKind.Object && modified.ValueKind == JsonValueKind.Object)
		{
			this.DiffObjects(original, modified, pointer);
		}
		else if (original.ValueKind == JsonValueKind.Array && modified.ValueKind == JsonValueKind.Array)
		{
			this.DiffArrays(original, modified, pointer);
		}
		else
		{
			// Either the kinds differ (e.g. object replaced by array) or both are scalars.
			// Compare canonical serializations so that object-value scalars with different
			// property orderings (which can't reach here, but defensive) still compare equal.
			string canonA = CanonicalJson.Serialize(original);
			string canonB = CanonicalJson.Serialize(modified);
			if (canonA != canonB)
			{
				this.AddOperation("replace", pointer, modified);
			}
		}
	}

	#endregion

	#region Private Methods

	private void DiffObjects(JsonElement original, JsonElement modified, string pointer)
	{
		// Build property maps for both sides.
		Dictionary<string, JsonElement> aProps = [];
		foreach (JsonProperty p in original.EnumerateObject())
		{
			aProps[p.Name] = p.Value;
		}

		Dictionary<string, JsonElement> bProps = [];
		foreach (JsonProperty p in modified.EnumerateObject())
		{
			bProps[p.Name] = p.Value;
		}

		// Collect the union of all names, sorted for deterministic output.
		SortedSet<string> allNames = new(StringComparer.Ordinal);
		foreach (string name in aProps.Keys)
		{
			allNames.Add(name);
		}

		foreach (string name in bProps.Keys)
		{
			allNames.Add(name);
		}

		foreach (string name in allNames)
		{
			string childPointer = JsonPointer.Append(pointer, name);
			bool inA = aProps.TryGetValue(name, out JsonElement aValue);
			bool inB = bProps.TryGetValue(name, out JsonElement bValue);

			if (inA && inB)
			{
				// Property exists in both — recurse to find fine-grained changes.
				this.Diff(aValue, bValue, childPointer);
			}
			else if (!inA && inB)
			{
				// Property added in modified.
				this.AddOperation("add", childPointer, bValue);
			}
			else if (inA && !inB)
			{
				// Property removed from original.
				this.AddOperation("remove", childPointer, value: null);
			}
		}
	}

	private void DiffArrays(JsonElement original, JsonElement modified, string pointer)
	{
		// Flatten both arrays to canonical JSON strings so MyersDiff<string> can compare them.
		List<JsonElement> aElems = [];
		foreach (JsonElement e in original.EnumerateArray())
		{
			aElems.Add(e);
		}

		List<JsonElement> bElems = [];
		foreach (JsonElement e in modified.EnumerateArray())
		{
			bElems.Add(e);
		}

		string[] aCanon = new string[aElems.Count];
		for (int i = 0; i < aElems.Count; i++)
		{
			aCanon[i] = CanonicalJson.Serialize(aElems[i]);
		}

		string[] bCanon = new string[bElems.Count];
		for (int i = 0; i < bElems.Count; i++)
		{
			bCanon[i] = CanonicalJson.Serialize(bElems[i]);
		}

		MyersDiff<string> diff = new(aCanon, bCanon, supportChangeEditType: true);
		EditScript script = diff.Execute();

		// Walk the EditScript in forward order, tracking how prior insert/delete ops have
		// shifted the live array so we emit the correct patch indices.
		//
		// insertionOffset accumulates the net shift applied to A-side indices:
		//   each Insert adds +1 (an element was inserted before later A elements)
		//   each Delete adds -1 (an element was removed, shifting later A elements left)
		//
		// For a Delete at A-index i: the current patch index is i + insertionOffset.
		// For an Insert at B-index j: the patch index IS j (B indices are already "live").
		// For a Change at A-index i / B-index j: recurse at patch index i + insertionOffset.
		int insertionOffset = 0;

		foreach (Edit edit in script)
		{
			if (edit.EditType == EditType.Delete)
			{
				for (int k = 0; k < edit.Length; k++)
				{
					int patchIndex = edit.StartA + k + insertionOffset;
					this.AddOperation("remove", JsonPointer.AppendIndex(pointer, patchIndex), value: null);
					insertionOffset--;
				}
			}
			else if (edit.EditType == EditType.Insert)
			{
				for (int k = 0; k < edit.Length; k++)
				{
					int bIndex = edit.StartB + k;
					this.AddOperation("add", JsonPointer.AppendIndex(pointer, bIndex), bElems[bIndex]);
					insertionOffset++;
				}
			}
			else if (edit.EditType == EditType.Change)
			{
				// Change means 1-to-1 pairing of delete+insert for the same length.
				// Recurse into each pair to get fine-grained ops rather than a coarse replace.
				for (int k = 0; k < edit.Length; k++)
				{
					int patchIndex = edit.StartA + k + insertionOffset;
					int bIndex = edit.StartB + k;
					string childPointer = JsonPointer.AppendIndex(pointer, patchIndex);

					int opsBefore = this.operations.Count;
					this.Diff(aElems[edit.StartA + k], bElems[bIndex], childPointer);

					// If recursion produced no ops, the canonical strings matched — nothing to do.
					// (This shouldn't happen because MyersDiff only emits a Change when canonicals differ.)
					_ = opsBefore; // suppress unused-variable warning
				}
			}
		}
	}

	private void AddOperation(string op, string path, JsonElement? value)
	{
		JsonObject operation = new()
		{
			["op"] = op,
			["path"] = path,
		};

		if (value.HasValue)
		{
			// Deep-clone the JsonElement into a JsonNode so the operation owns its value.
			operation["value"] = JsonNode.Parse(value.Value.GetRawText());
		}

		this.operations.Add(operation);
	}

	#endregion
}
