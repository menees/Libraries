namespace Menees.Diffs.Json;

#region Using Directives

using System.Text.Json;
using System.Text.Json.Nodes;

#endregion

/// <summary>
/// Generates RFC 6902 JSON Patch documents by diffing two JSON values using
/// the Myers diff algorithm.
/// </summary>
/// <remarks>
/// <para>
/// The returned <see cref="JsonArray"/> contains RFC 6902 operation objects
/// (<c>op</c>, <c>path</c>, and optionally <c>value</c>).  Paths use RFC 6901
/// JSON Pointer notation (e.g. <c>/address/city</c>, <c>/tags/0</c>).
/// </para>
/// <para>
/// The generated patch uses only <c>add</c>, <c>remove</c>, and <c>replace</c>
/// operations, which are the three operations that map directly onto the
/// Myers diff <c>Insert</c>, <c>Delete</c>, and <c>Change</c> <see cref="EditType"/>s.
/// </para>
/// <para>
/// Callers using ASP.NET Core can round-trip the result to a
/// <c>JsonPatchDocument&lt;T&gt;</c> by deserializing the JSON string:
/// <code>
/// var patch = JsonDiff.CreatePatch(originalJson, modifiedJson);
/// var patchDoc = JsonSerializer.Deserialize&lt;JsonPatchDocument&lt;MyModel&gt;&gt;(patch.ToJsonString());
/// </code>
/// </para>
/// </remarks>
public static class JsonDiff
{
	#region Public Methods

	/// <summary>
	/// Computes a JSON Patch document that transforms <paramref name="original"/>
	/// into <paramref name="modified"/>.
	/// </summary>
	/// <param name="original">The original JSON value.</param>
	/// <param name="modified">The modified JSON value.</param>
	/// <returns>
	/// A <see cref="JsonArray"/> of RFC 6902 operation objects.
	/// The array is empty when the two values are identical.
	/// </returns>
	public static JsonArray CreatePatch(JsonElement original, JsonElement modified)
	{
		JsonPatchBuilder builder = new();
		builder.Diff(original, modified, JsonPointer.Root);

		JsonArray result = [];
		foreach (JsonObject op in builder.Operations)
		{
			result.Add((JsonNode)op);
		}

		return result;
	}

	/// <summary>
	/// Computes a JSON Patch document that transforms <paramref name="originalJson"/>
	/// into <paramref name="modifiedJson"/>.
	/// </summary>
	/// <param name="originalJson">The original JSON text.</param>
	/// <param name="modifiedJson">The modified JSON text.</param>
	/// <returns>
	/// A <see cref="JsonArray"/> of RFC 6902 operation objects.
	/// The array is empty when the two values are identical.
	/// </returns>
	public static JsonArray CreatePatch(string originalJson, string modifiedJson)
	{
		using JsonDocument originalDoc = JsonDocument.Parse(originalJson);
		using JsonDocument modifiedDoc = JsonDocument.Parse(modifiedJson);
		return CreatePatch(originalDoc.RootElement, modifiedDoc.RootElement);
	}

	/// <summary>
	/// Computes a JSON Patch document and returns it serialized as a JSON string.
	/// </summary>
	/// <param name="original">The original JSON value.</param>
	/// <param name="modified">The modified JSON value.</param>
	/// <returns>A JSON string containing the RFC 6902 patch array.</returns>
	public static string CreatePatchText(JsonElement original, JsonElement modified)
	{
		return CreatePatch(original, modified).ToJsonString();
	}

	/// <summary>
	/// Computes a JSON Patch document and returns it serialized as a JSON string.
	/// </summary>
	/// <param name="originalJson">The original JSON text.</param>
	/// <param name="modifiedJson">The modified JSON text.</param>
	/// <returns>A JSON string containing the RFC 6902 patch array.</returns>
	public static string CreatePatchText(string originalJson, string modifiedJson)
	{
		return CreatePatch(originalJson, modifiedJson).ToJsonString();
	}

	#endregion
}
