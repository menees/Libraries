namespace Menees.Diffs.Json;

/// <summary>
/// Helpers for building RFC 6901 JSON Pointer strings.
/// </summary>
/// <remarks>
/// RFC 6901 requires that '~' is escaped as '~0' and '/' is escaped as '~1'
/// (in that order, so that '~1' doesn't double-escape to '~01').
/// The root pointer is the empty string "".
/// </remarks>
internal static class JsonPointer
{
	#region Public Constants

	public const string Root = "";

	#endregion

	#region Public Methods

	/// <summary>
	/// Escapes a single path segment per RFC 6901:
	/// '~' → '~0', '/' → '~1'.
	/// </summary>
	public static string Escape(string segment)
	{
		// '~' must be replaced before '/' to avoid double-escaping.
		return segment.Replace("~", "~0").Replace("/", "~1");
	}

	/// <summary>
	/// Appends an escaped segment to an existing pointer, separated by '/'.
	/// </summary>
	public static string Append(string pointer, string segment)
	{
		return pointer + "/" + Escape(segment);
	}

	/// <summary>
	/// Appends a numeric array index to an existing pointer.
	/// Indices are never escaped because they contain only digits.
	/// </summary>
	public static string AppendIndex(string pointer, int index)
	{
		return pointer + "/" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
	}

	#endregion
}
