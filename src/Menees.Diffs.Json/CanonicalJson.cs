namespace Menees.Diffs.Json;

#region Using Directives

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

#endregion

/// <summary>
/// Produces a canonical JSON string from a <see cref="JsonElement"/> by
/// re-serializing it with object properties sorted by name (ordinal, ascending).
/// This gives <see cref="MyersDiff{T}"/> a stable, order-independent key for
/// comparing JSON objects inside arrays.
/// </summary>
internal static class CanonicalJson
{
	#region Private Constants

	private static readonly JsonWriterOptions WriterOptions = new() { Indented = false };

	#endregion

	#region Public Methods

	/// <summary>
	/// Returns a canonical JSON string for <paramref name="element"/>.
	/// Object properties are sorted by name using ordinal comparison at every level.
	/// All other JSON kinds are serialized without modification.
	/// </summary>
	public static string Serialize(JsonElement element)
	{
		using MemoryStream stream = new();
		using (Utf8JsonWriter writer = new(stream, WriterOptions))
		{
			WriteElement(writer, element);
		}

		return Encoding.UTF8.GetString(stream.ToArray());
	}

	#endregion

	#region Private Methods

	private static void WriteElement(Utf8JsonWriter writer, JsonElement element)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.Object:
				WriteObject(writer, element);
				break;

			case JsonValueKind.Array:
				WriteArray(writer, element);
				break;

			case JsonValueKind.String:
				writer.WriteStringValue(element.GetString());
				break;

			case JsonValueKind.Number:
				// Preserve the raw number text to avoid any floating-point round-trip issues.
				writer.WriteRawValue(element.GetRawText());
				break;

			case JsonValueKind.True:
				writer.WriteBooleanValue(true);
				break;

			case JsonValueKind.False:
				writer.WriteBooleanValue(false);
				break;

			case JsonValueKind.Null:
			default:
				writer.WriteNullValue();
				break;
		}
	}

	private static void WriteObject(Utf8JsonWriter writer, JsonElement element)
	{
		writer.WriteStartObject();

		// Sort properties by name (ordinal) so that objects with the same
		// properties in different orders compare as equal.
		IEnumerable<JsonProperty> sorted = element.EnumerateObject()
			.OrderBy(p => p.Name, StringComparer.Ordinal);

		foreach (JsonProperty property in sorted)
		{
			writer.WritePropertyName(property.Name);
			WriteElement(writer, property.Value);
		}

		writer.WriteEndObject();
	}

	private static void WriteArray(Utf8JsonWriter writer, JsonElement element)
	{
		writer.WriteStartArray();

		foreach (JsonElement item in element.EnumerateArray())
		{
			WriteElement(writer, item);
		}

		writer.WriteEndArray();
	}

	#endregion
}
