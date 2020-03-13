namespace Menees
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.ExceptionServices;
	using System.Text;

	#endregion

	/// <summary>
	/// Methods for converting data types from one form to another.
	/// </summary>
	public static class ConvertUtility
	{
		#region Private Data Members

		private static readonly HashSet<string> FalseValues = new HashSet<string>(
			new string[] { "false", "f", "no", "n", "0" }, StringComparer.OrdinalIgnoreCase);

		private static readonly HashSet<string> TrueValues = new HashSet<string>(
			new string[] { "true", "t", "yes", "y", "1" }, StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Public Methods

		/// <summary>
		/// Converts a value into the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the value into.</typeparam>
		/// <param name="value">The value to convert.</param>
		/// <returns>The converted value.</returns>
		/// <exception cref="InvalidCastException">If <paramref name="value"/>
		/// can't be converted into type T.</exception>
		public static T ConvertValue<T>(object value)
		{
			T result;

			// We can't do a C# "as" check because T may not be a reference type.
			// There's a small amount of duplicate work done by "is + Cast" (because
			// .NET will do the "is" check again during the cast), but it's unavoidable
			// in this case because we're using generics with no class constraint.
			if (value is T)
			{
				result = (T)value;
			}
			else
			{
				result = (T)ConvertValue(value, typeof(T));
			}

			return result;
		}

		/// <summary>
		/// Converts a value into the specified type.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="resultType">The type to convert the value into.</param>
		/// <returns>The converted value.</returns>
		/// <exception cref="InvalidCastException">If <paramref name="value"/>
		/// can't be converted into type T.</exception>
		public static object ConvertValue(object value, Type resultType)
		{
			Conditions.RequireReference(resultType, nameof(resultType));

			bool converted = false;
			object result = null;

			if (value != null)
			{
				// Try using a TypeConverter instead of Convert.ChangeType because TypeConverter supports
				// a lot more types (including enums and nullable types).  Convert.ChangeType only supports
				// IConvertible (see http://aspalliance.com/852).
				TypeConverter converter = TypeDescriptor.GetConverter(resultType);
				if (converter.CanConvertFrom(value.GetType()))
				{
					try
					{
						object fromTypeConverter = converter.ConvertFrom(value);
						result = fromTypeConverter;
						converted = true;
					}
					catch (Exception ex)
					{
						// .NET's System.ComponentModel.BaseNumberConverter.ConvertFrom method catches
						// all exceptions and then rethrows a System.Exception, which is HORRIBLE!  We'll try to
						// undo Microsoft's mistake by re-throwing the original exception, so callers can catch
						// specific exception types.
						Exception inner = ex.InnerException;
						if (inner != null)
						{
							ExceptionDispatchInfo.Capture(inner).Throw();
						}

						throw;
					}
				}
			}
			else if (!resultType.IsValueType || Nullable.GetUnderlyingType(resultType) != null)
			{
				// http://stackoverflow.com/questions/374651/how-to-check-if-an-object-is-nullable
				result = null;
				converted = true;
			}

			if (!converted)
			{
				// The value was null (for a value type) or the type converter couldn't convert it,
				// so we have to fall back to ChangeType.
				result = Convert.ChangeType(value, resultType);
			}

			return result;
		}

		/// <summary>
		/// Gets whether a value is a null reference or DBNull.Value.
		/// </summary>
		/// <param name="value">The value to check.</param>
		/// <returns>True if the value is null or DBNull.Value.  False otherwise.</returns>
		public static bool IsNull(object value)
		{
			bool result = value == null || value == DBNull.Value;
			return result;
		}

		/// <summary>
		/// Converts a string value to a bool.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <returns>True if the string case-insensitively matches "True", "T", "Yes", "Y", or "1".  False otherwise.</returns>
		public static bool ToBoolean(string value)
		{
			bool result = TrueValues.Contains(value);
			return result;
		}

		/// <summary>
		/// Converts a string value to a bool.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="defaultValue">The default value to use if the string can't be converted.</param>
		/// <returns>
		/// True if the string case-insensitively matches "True", "T", "Yes", "Y", or "1".
		/// False if the string case-insensitively matches "False", "F", "No", "N", or "0".
		/// If the value is not one of the true values and not one of the false values,
		/// then this returns the defaultValue.  For example, ToBoolean("P", true) will return true,
		/// and ToBoolean("Q", false) will return false.
		/// </returns>
		public static bool ToBoolean(string value, bool defaultValue)
		{
			bool result = defaultValue;

			if (TrueValues.Contains(value))
			{
				result = true;
			}
			else
			{
				if (FalseValues.Contains(value))
				{
					result = false;
				}
			}

			return result;
		}

		/// <summary>
		/// Converts a string value to an int.
		/// </summary>
		/// <param name="value">The string to convert.</param>
		/// <param name="defaultValue">The default value to use if the string can't be converted.</param>
		/// <returns>The int value.</returns>
		public static int ToInt32(string value, int defaultValue)
		{
			int result = defaultValue;

			if (int.TryParse(value, out int parsed))
			{
				result = parsed;
			}

			return result;
		}

		/// <summary>
		/// Converts a sequence of bytes into hexadecimal nibbles with an optional "0x" prefix.
		/// </summary>
		/// <param name="value">The sequence of bytes to convert.</param>
		/// <param name="options">Options affecting a "0x" prefix and whether to use lowercase hex characters.</param>
		/// <returns>The encoded hex bytes.</returns>
		public static string ToHex(IEnumerable<byte> value, ToHexOptions options = ToHexOptions.None)
		{
			string result = null;

			if (value != null)
			{
				string prefix = options.HasFlag(ToHexOptions.Include0xPrefix) ? "0x" : string.Empty;
				StringBuilder sb = new StringBuilder(prefix, prefix.Length + (2 * value.Count()));

				string format = options.HasFlag(ToHexOptions.Lowercase) ? "{0:x2}" : "{0:X2}";
				foreach (byte entry in value)
				{
					sb.AppendFormat(CultureInfo.InvariantCulture, format, entry);
				}

				result = sb.ToString();
			}

			return result;
		}

		/// <summary>
		/// Tries to parse a string of hexadecimal characters into a byte array.
		/// </summary>
		/// <param name="value">A string of hex characters. This can optionally
		/// start with a "0x" prefix and contain colons or whitespace.</param>
		/// <param name="throwOnError">Whether an exception should be thrown for invalid input.
		/// If false, then a null result will be returned for invalid input.
		/// </param>
		/// <exception cref="ArgumentException">Thrown if the input is invalid and
		/// <paramref name="throwOnError"/> is true.</exception>
		/// <returns>A byte array if <paramref name="value"/> can be parsed.
		/// Or null if <paramref name="value"/>can't be parsed and <paramref name="throwOnError"/> is false.</returns>
		public static byte[] FromHex(string value, bool throwOnError = true)
		{
			byte[] result = null;

			if (value != null)
			{
				string errorMessage = null;

				// Ignore leading and trailing whitespace, embedded whitespace, and colon separators (used in certificate hashes).
				List<char> chars = value.Where(ch => !char.IsWhiteSpace(ch) && ch != ':').ToList();

				// Skip a "0x" prefix.
				int charCount = chars.Count;
				int startIndex = (charCount >= 2 && chars[0] == '0' && (chars[1] == 'x' || chars[1] == 'X')) ? 2 : 0;

				// If we see an odd number of nibbles (e.g., in 0x123), then add a leading 0 to make it 0x0123.
				if (charCount % 2 != 0)
				{
					chars.Insert(startIndex, '0');
					charCount++;
				}

				// Use a MemoryStream instead of List<byte> so we can grab its internal buffer at the end
				// without a realloc if we specify an initial capacity that matches its final length.
				int capacity = (charCount - startIndex) / 2;
				using (MemoryStream stream = new MemoryStream(capacity))
				{
					byte[] buffer = new byte[1];
					for (int i = startIndex; i < charCount; i += 2)
					{
						// This isn't super-efficient, but it's simple to understand.
						string byteText = $"{chars[i]}{chars[i + 1]}";
						if (!byte.TryParse(byteText, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out buffer[0]))
						{
							errorMessage = "Invalid hex byte representation: " + byteText;
							break;
						}
						else
						{
							stream.Write(buffer, 0, 1);
						}
					}

					if (string.IsNullOrEmpty(errorMessage))
					{
						result = (stream.Length == stream.Capacity) ? stream.GetBuffer() : stream.ToArray();
					}
				}

				if (!string.IsNullOrEmpty(errorMessage) && throwOnError)
				{
					throw Exceptions.NewArgumentException(errorMessage);
				}
			}

			return result;
		}

		#endregion

		#region Internal Methods

		internal static T GetValue<T>(string textValue, T defaultValue)
			where T : struct
		{
			T result = defaultValue;
			if (!string.IsNullOrEmpty(textValue))
			{
				Type type = typeof(T);

				// Enums are the most common case, so handle them specially.
				if (type.IsEnum)
				{
					if (Enum.TryParse<T>(textValue, out T parsedValue))
					{
						result = parsedValue;
					}
				}
				else
				{
					// Handle other simple types like Double and DateTime along with types
					// that support TypeConverters (e.g., System.Windows.Forms.Color).
					try
					{
						result = ConvertValue<T>(textValue);
					}
					catch (Exception ex)
					{
						if (!(ex is ArgumentException || ex is ArithmeticException || ex is FormatException || ex is IndexOutOfRangeException))
						{
							throw;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}
