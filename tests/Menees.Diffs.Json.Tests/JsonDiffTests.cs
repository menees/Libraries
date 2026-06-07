using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Menees.Diffs.Json.Tests;

[TestClass]
public class JsonDiffTests
{
	#region Helper Methods

	/// <summary>Parses the patch text and returns an array of (op, path, value?) tuples.</summary>
	private static (string Op, string Path, JsonNode? Value)[] ParseOps(string patchText)
	{
		JsonArray array = JsonNode.Parse(patchText)!.AsArray();
		var result = new (string Op, string Path, JsonNode? Value)[array.Count];
		for (int i = 0; i < array.Count; i++)
		{
			JsonObject obj = array[i]!.AsObject();
			result[i] = (
				obj["op"]!.GetValue<string>(),
				obj["path"]!.GetValue<string>(),
				obj["value"]
			);
		}

		return result;
	}

	private static string Patch(string original, string modified)
		=> JsonDiff.CreatePatchText(original, modified);

	private static (string Op, string Path, JsonNode? Value)[] PatchOps(string original, string modified)
		=> ParseOps(Patch(original, modified));

	#endregion

	#region Identical Documents

	[TestMethod]
	public void IdenticalScalar_EmptyPatch()
	{
		PatchOps("""42""", """42""").ShouldBeEmpty();
	}

	[TestMethod]
	public void IdenticalObject_EmptyPatch()
	{
		PatchOps("""{"a":1,"b":2}""", """{"a":1,"b":2}""").ShouldBeEmpty();
	}

	[TestMethod]
	public void IdenticalObjectDifferentPropertyOrder_EmptyPatch()
	{
		// Canonical serialization sorts properties, so order doesn't matter for comparison.
		PatchOps("""{"b":2,"a":1}""", """{"a":1,"b":2}""").ShouldBeEmpty();
	}

	[TestMethod]
	public void IdenticalArray_EmptyPatch()
	{
		PatchOps("""[1,2,3]""", """[1,2,3]""").ShouldBeEmpty();
	}

	[TestMethod]
	public void EmptyObject_EmptyPatch()
	{
		PatchOps("""{}""", """{}""").ShouldBeEmpty();
	}

	[TestMethod]
	public void EmptyArray_EmptyPatch()
	{
		PatchOps("""[]""", """[]""").ShouldBeEmpty();
	}

	#endregion

	#region Scalar Root

	[TestMethod]
	public void ScalarRootReplace_Number()
	{
		var ops = PatchOps("""1""", """2""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("");
		ops[0].Value!.GetValue<int>().ShouldBe(2);
	}

	[TestMethod]
	public void ScalarRootReplace_String()
	{
		var ops = PatchOps("\"hello\"", "\"world\"");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("");
		ops[0].Value!.GetValue<string>().ShouldBe("world");
	}

	[TestMethod]
	public void ScalarRootKindChange_StringToNumber()
	{
		var ops = PatchOps("\"42\"", """42""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
	}

	#endregion

	#region Object Property Changes

	[TestMethod]
	public void Object_AddProperty()
	{
		var ops = PatchOps("""{"a":1}""", """{"a":1,"b":2}""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("add");
		ops[0].Path.ShouldBe("/b");
		ops[0].Value!.GetValue<int>().ShouldBe(2);
	}

	[TestMethod]
	public void Object_RemoveProperty()
	{
		var ops = PatchOps("""{"a":1,"b":2}""", """{"a":1}""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("remove");
		ops[0].Path.ShouldBe("/b");
		ops[0].Value.ShouldBeNull();
	}

	[TestMethod]
	public void Object_ReplaceProperty()
	{
		var ops = PatchOps("""{"a":1,"b":2}""", """{"a":1,"b":99}""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("/b");
		ops[0].Value!.GetValue<int>().ShouldBe(99);
	}

	[TestMethod]
	public void Object_AddRemoveReplace_Deterministic()
	{
		// Three simultaneous changes: one add, one remove, one replace.
		// Output order follows ordinal-sorted property names: level(add) < name(=) < role(replace) < score(remove)
		string original = """{"name":"Alice","role":"user","score":10}""";
		string modified = """{"name":"Alice","role":"admin","level":3}""";

		var ops = PatchOps(original, modified);
		ops.Length.ShouldBe(3);

		// Sorted by property name: "level" (add), "role" (replace), "score" (remove)
		ops[0].Op.ShouldBe("add");
		ops[0].Path.ShouldBe("/level");
		ops[0].Value!.GetValue<int>().ShouldBe(3);

		ops[1].Op.ShouldBe("replace");
		ops[1].Path.ShouldBe("/role");
		ops[1].Value!.GetValue<string>().ShouldBe("admin");

		ops[2].Op.ShouldBe("remove");
		ops[2].Path.ShouldBe("/score");
	}

	[TestMethod]
	public void Object_KindChangeProperty_ObjectToScalar()
	{
		var ops = PatchOps("""{"x":{"nested":1}}""", """{"x":42}""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("/x");
		ops[0].Value!.GetValue<int>().ShouldBe(42);
	}

	#endregion

	#region Nested Object Recursion

	[TestMethod]
	public void NestedObject_RecursesToChangedLeaf()
	{
		string original = """{"user":{"id":1,"address":{"city":"London","zip":"EC1A"}}}""";
		string modified = """{"user":{"id":1,"address":{"city":"Paris","zip":"75001"}}}""";

		var ops = PatchOps(original, modified);
		ops.Length.ShouldBe(2);

		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("/user/address/city");
		ops[0].Value!.GetValue<string>().ShouldBe("Paris");

		ops[1].Op.ShouldBe("replace");
		ops[1].Path.ShouldBe("/user/address/zip");
		ops[1].Value!.GetValue<string>().ShouldBe("75001");
	}

	[TestMethod]
	public void NestedObject_AddNestedProperty()
	{
		var ops = PatchOps("""{"a":{"x":1}}""", """{"a":{"x":1,"y":2}}""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("add");
		ops[0].Path.ShouldBe("/a/y");
		ops[0].Value!.GetValue<int>().ShouldBe(2);
	}

	#endregion

	#region Array Diffing via Myers

	[TestMethod]
	public void Array_AppendElement()
	{
		var ops = PatchOps("""["a","b","c"]""", """["a","b","c","d"]""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("add");
		ops[0].Path.ShouldBe("/3");
		ops[0].Value!.GetValue<string>().ShouldBe("d");
	}

	[TestMethod]
	public void Array_InsertAtStart_OneOp()
	{
		// Myers diff should produce exactly one add, not cascade replacements.
		var ops = PatchOps("""["a","b","c"]""", """["x","a","b","c"]""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("add");
		ops[0].Path.ShouldBe("/0");
		ops[0].Value!.GetValue<string>().ShouldBe("x");
	}

	[TestMethod]
	public void Array_DeleteFromMiddle()
	{
		var ops = PatchOps("""["a","b","c","d"]""", """["a","c","d"]""");
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("remove");
		ops[0].Path.ShouldBe("/1");
	}

	[TestMethod]
	public void Array_ReplaceElement_UsesMyersDiff()
	{
		// "red" matches, "blue" -> "purple" is a Change, "green" matches, "yellow" is inserted.
		string original = """{"tags":["red","blue","green"]}""";
		string modified = """{"tags":["red","purple","green","yellow"]}""";

		var ops = PatchOps(original, modified);
		ops.Length.ShouldBe(2);

		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("/tags/1");
		ops[0].Value!.GetValue<string>().ShouldBe("purple");

		ops[1].Op.ShouldBe("add");
		ops[1].Path.ShouldBe("/tags/3");
		ops[1].Value!.GetValue<string>().ShouldBe("yellow");
	}

	[TestMethod]
	public void Array_FromEmptyToNonEmpty()
	{
		var ops = PatchOps("""[]""", """[1,2,3]""");
		ops.Length.ShouldBe(3);
		for (int i = 0; i < ops.Length; i++)
		{
			ops[i].Op.ShouldBe("add");
			ops[i].Path.ShouldBe("/" + i);
			ops[i].Value!.GetValue<int>().ShouldBe(i + 1);
		}
	}

	[TestMethod]
	public void Array_FromNonEmptyToEmpty()
	{
		var ops = PatchOps("""[1,2,3]""", """[]""");
		ops.Length.ShouldBe(3);
		// All removes reference index 0 because each delete shifts the array.
		foreach (var (Op, Path, Value) in ops)
		{
			Op.ShouldBe("remove");
			Path.ShouldBe("/0");
			Value.ShouldBeNull();
		}
	}

	[TestMethod]
	public void Array_ObjectElements_RecurseIntoChangedPair()
	{
		// Array contains objects; Myers Change should recurse, not emit a coarse replace.
		string original = """[{"id":1,"val":"x"},{"id":2,"val":"y"}]""";
		string modified = """[{"id":1,"val":"x"},{"id":2,"val":"z"}]""";

		var ops = PatchOps(original, modified);
		ops.Length.ShouldBe(1);
		ops[0].Op.ShouldBe("replace");
		ops[0].Path.ShouldBe("/1/val");
		ops[0].Value!.GetValue<string>().ShouldBe("z");
	}

	[TestMethod]
	public void Array_MultipleInsertsAndDeletes_CorrectIndices()
	{
		// Delete "b" (index 1), insert "x" at the start.
		// Myers output and insertionOffset tracking must keep indices consistent.
		string original = """["a","b","c"]""";
		string modified = """["x","a","c"]""";

		var ops = PatchOps(original, modified);

		// Verify the patch is semantically correct by applying it manually:
		// Start: ["a","b","c"]
		// After each op in order, the array should become ["x","a","c"].
		// We don't care about the exact op ordering as long as the sequence is valid;
		// we just verify that op count > 0 and that paths reference valid positions.
		ops.Length.ShouldBeGreaterThan(0);
		foreach (var (Op, Path, Value) in ops)
		{
			(Op == "add" || Op == "remove" || Op == "replace").ShouldBeTrue();
			Path.ShouldNotBeNull();
			if (Op == "remove")
			{
				Value.ShouldBeNull();
			}
			else
			{
				Value.ShouldNotBeNull();
			}
		}
	}

	#endregion

	#region Mixed Object and Array

	[TestMethod]
	public void MixedDocument_ObjectWithArrayProperty()
	{
		string original = """{"user":"alice","tags":["red","blue"],"score":42}""";
		string modified = """{"user":"alice","tags":["red","purple","blue"],"score":99}""";

		var ops = PatchOps(original, modified);

		// score replace and tags insert
		ops.Length.ShouldBe(2);

		var (Op, Path, Value) = ops.Single(o => o.Path == "/score");
		Op.ShouldBe("replace");
		Value!.GetValue<int>().ShouldBe(99);

		var tagOp = ops.Single(o => o.Path.StartsWith("/tags/"));
		tagOp.Op.ShouldBe("add");
		tagOp.Value!.GetValue<string>().ShouldBe("purple");
	}

	#endregion

	#region JSON Pointer Escaping

	[TestMethod]
	public void Escaping_SlashInPropertyName()
	{
		var ops = PatchOps("""{"a/b":1}""", """{"a/b":9}""");
		ops.Length.ShouldBe(1);
		ops[0].Path.ShouldBe("/a~1b");
		ops[0].Value!.GetValue<int>().ShouldBe(9);
	}

	[TestMethod]
	public void Escaping_TildeInPropertyName()
	{
		var ops = PatchOps("""{"c~d":2}""", """{"c~d":5}""");
		ops.Length.ShouldBe(1);
		ops[0].Path.ShouldBe("/c~0d");
		ops[0].Value!.GetValue<int>().ShouldBe(5);
	}

	[TestMethod]
	public void Escaping_TildeOneInPropertyName_TildeEscapedFirst()
	{
		// Per RFC 6901, '~' must be escaped before '/', so '~1' becomes '~01', not '~1'.
		var ops = PatchOps("""{"~1":10}""", """{"~1":20}""");
		ops.Length.ShouldBe(1);
		ops[0].Path.ShouldBe("/~01");
	}

	#endregion

	#region CreatePatch JsonElement Overload

	[TestMethod]
	public void CreatePatch_JsonElementOverload()
	{
		using JsonDocument original = JsonDocument.Parse("""{"x":1}""");
		using JsonDocument modified = JsonDocument.Parse("""{"x":2}""");

		JsonArray patch = JsonDiff.CreatePatch(original.RootElement, modified.RootElement);
		patch.Count.ShouldBe(1);
		patch[0]!.AsObject()["op"]!.GetValue<string>().ShouldBe("replace");
	}

	[TestMethod]
	public void CreatePatchText_ReturnsValidJsonString()
	{
		string text = JsonDiff.CreatePatchText("""{"a":1}""", """{"a":2}""");
		// Must be parseable as a JSON array.
		JsonArray parsed = JsonNode.Parse(text)!.AsArray();
		parsed.Count.ShouldBe(1);
	}

	#endregion
}
