namespace Menees.Diffs
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Text;
	using System.Xml;
	using Menees;
	using Microsoft.Win32;

	#endregion

	/// <summary>
	/// Provides helper routines for working with files.
	/// </summary>
	public static class DiffUtility
	{
		#region Public Members

		public static bool AreFilesDifferent(string fileName1, string fileName2) => AreFilesDifferent(new FileInfo(fileName1), new FileInfo(fileName2));

		public static bool AreFilesDifferent(FileInfo info1, FileInfo info2)
		{
			bool result = true;

			// Before we open the files, compare the sizes.  If they are different, then the files are certainly different.
			if (info1.Length == info2.Length)
			{
				using (FileStream stream1 = info1.OpenRead())
				using (FileStream stream2 = info2.OpenRead())
				{
					// The previous length check should ensure these are equal.
					Debug.Assert(stream1.Length == stream2.Length, "The streams' lengths must be the same.");

					// They have the same lengths, so we have to check byte-by-byte.  As soon as we find a difference, we can quit.
					// Start by assuming the files are byte-by-byte equal.
					result = false;
					int byte1, byte2;
					do
					{
						byte1 = stream1.ReadByte();
						byte2 = stream2.ReadByte();

						if (byte1 != byte2)
						{
							result = true;
							break;
						}
					}
					while (byte1 >= 0 && byte2 >= 0);
				}
			}

			return result;
		}

		public static IList<string> GetFileTextLines(string fileName)
		{
			using (StreamReader reader = new(fileName, Encoding.Default, true))
			{
				return GetTextLines(reader);
			}
		}

		public static IList<string> GetStringTextLines(string text)
		{
			using (StringReader reader = new(text))
			{
				return GetTextLines(reader);
			}
		}

		public static IList<string> GetTextLines(TextReader reader)
		{
			IList<string> result = new List<string>();

			while (reader.Peek() > -1)
			{
				string? line = reader.ReadLine();
				if (line != null)
				{
					result.Add(line);
				}
			}

			return result;
		}

		public static IList<string> GetXmlTextLines(string fileName, bool ignoreInsignificantWhiteSpace)
		{
			using (StreamReader reader = new(fileName, Encoding.Default, true))
			{
				return GetXmlTextLines(reader, ignoreInsignificantWhiteSpace);
			}
		}

		public static IList<string> GetXmlTextLines(XmlReader reader)
		{
			XmlWriterSettings settings = new()
			{
				CheckCharacters = false,
				CloseOutput = true,
				Indent = true,
				IndentChars = "\t",
				NewLineOnAttributes = true,
			};

			StringBuilder sb = new();
			using (XmlWriter writer = XmlWriter.Create(sb, settings))
			{
				writer.WriteNode(reader, false);
			}

			IList<string> result = GetStringTextLines(sb.ToString());
			return result;
		}

		public static IList<string> GetXmlTextLinesFromXml(string xml, bool ignoreInsignificantWhiteSpace)
		{
			using (StringReader reader = new(xml))
			{
				return GetXmlTextLines(reader, ignoreInsignificantWhiteSpace);
			}
		}

		public static bool IsBinaryFile(string fileName)
		{
			using (FileStream stream = File.OpenRead(fileName))
			{
				return IsBinaryFile(stream);
			}
		}

		public static bool IsBinaryFile(Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);

			// First see if the file begins with any known Unicode byte order marks.
			// If so, then it is a text file.  Use a StreamReader instance to do the
			// auto-detection logic.
			bool detectedEncoding;
			const int BufferSize = 1024;
			using (StreamReader reader = new(stream, Encoding.Default, true, BufferSize, leaveOpen: true))
			{
				reader.Read(); // We have to force a Read for it to auto-detect.
				detectedEncoding = reader.CurrentEncoding != Encoding.Default;
			}

			bool result = false;
			if (!detectedEncoding)
			{
				// Since the type was Default encoding, that means there were no byte-order
				// marks to indicate its type.  So we have to scan.  If we find a NULL
				// character in the stream, that means it's a binary file.
				stream.Seek(0, SeekOrigin.Begin);
				int i;
				while ((i = stream.ReadByte()) > -1)
				{
					if (i == 0)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Private Methods

		private static IList<string> GetXmlTextLines(TextReader textReader, bool ignoreInsignificantWhitespace)
		{
			XmlReaderSettings settings = new()
			{
				CheckCharacters = false,
				CloseInput = true,
				DtdProcessing = DtdProcessing.Ignore,
				IgnoreWhitespace = ignoreInsignificantWhitespace,
				ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.None,
				ValidationType = ValidationType.None,
				XmlResolver = null,
			};

			using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
			{
				IList<string> result = GetXmlTextLines(xmlReader);
				return result;
			}
		}

		#endregion
	}
}
