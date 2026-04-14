using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class TextUtilityTest
	{
		[TestMethod()]
		public void GetPrintableCharacterTest()
		{
			char actual = TextUtility.GetPrintableCharacter('\r');
			actual.ShouldBe('\u2190'); // Left arrow
			actual = TextUtility.GetPrintableCharacter('\n');
			actual.ShouldBe('\u2193'); // Down arrow
			actual = TextUtility.GetPrintableCharacter('\0');
			actual.ShouldBe('\u03D5'); // Phi/nil

			actual = TextUtility.GetPrintableCharacter('A');
			actual.ShouldBe('A');
		}

		[TestMethod()]
		public void EnsureQuotesTest()
		{
			string actual = TextUtility.EnsureQuotes("Test");
			actual.ShouldBe("\"Test\"");
			actual = TextUtility.EnsureQuotes("Test", "'");
			actual.ShouldBe("'Test'");
			actual = TextUtility.EnsureQuotes("Test", "[", "]");
			actual.ShouldBe("[Test]");
			actual = TextUtility.EnsureQuotes(actual, "[", "]");
			actual.ShouldBe("[Test]");
		}

		[TestMethod()]
		public void ReplaceTest()
		{
			string actual = TextUtility.Replace("Testing tester TESTED tests.", "Test", "Record", StringComparison.CurrentCultureIgnoreCase);
			actual.ShouldBe("Recording Recorder RecordED Records.");

			actual = TextUtility.Replace("Testing tester TESTED tests.", "Test", "Buy", StringComparison.CurrentCultureIgnoreCase);
			actual.ShouldBe("Buying Buyer BuyED Buys.");

			string text = "This has no matches.";
			actual = TextUtility.Replace(text, "ABCD", "wxyz", StringComparison.OrdinalIgnoreCase);
			actual.ShouldBe(text);
		}

		[TestMethod()]
		public void ReplaceControlCharactersTest()
		{
			string actual = TextUtility.ReplaceControlCharacters("Line one\r\nLine two\0", ' ');
			actual.ShouldBe("Line one  Line two ");

			actual = TextUtility.ReplaceControlCharacters("Line one\r\nLine two\0");
			actual.ShouldBe("Line one\u2190\u2193Line two\u03D5");
		}

		[TestMethod]
		public void SplitIntoTokensNoDelimiterTest()
		{
			string[] actual = [.. TextUtility.SplitIntoTokens("A, B, C, D", ',', null, true)];
			CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, actual);

			// The double quotes here should be part of the the token since we're not using a delimiter.
			actual = [.. TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D", ',', null, true)];
			CollectionAssert.AreEqual(new[] { "A", "B", "\"C1", "C2\"", "D" }, actual);

			// Do the same thing but don't trim the tokens.
			actual = [.. TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D", ',', null, false)];
			CollectionAssert.AreEqual(new[] { "A", " B", " \"C1", " C2\"", " D" }, actual);

			// Test with leading and trailing empty tokens.
			actual = [.. TextUtility.SplitIntoTokens(", , ,", ',', null, false)];
			CollectionAssert.AreEqual(new[] { "", " ", " ", "" }, actual);

			// Test with all empty tokens (where the two middle tokens get trimmed).
			actual = [.. TextUtility.SplitIntoTokens(", , ,", ',', null, true)];
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);
		}

		[TestMethod]
		public void SplitIntoTokensListTest()
		{
			string[] actual = [.. TextUtility.SplitIntoTokens("A, B, C")];
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, actual);

			actual = [.. TextUtility.SplitIntoTokens(" A , B , C ")];
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, actual);

			actual = [.. TextUtility.SplitIntoTokens(",, ,")];
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);

			actual = [.. TextUtility.SplitIntoTokens("a=A; b=B;' c=C;See '; d=D", ';', '\'', true)];
			CollectionAssert.AreEqual(new[] { "a=A", "b=B", "c=C;See", "d=D" }, actual);

			// Test with space before a delimiter.
			actual = [.. TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D")];
			CollectionAssert.AreEqual(new[] { "A", "B", "C1, C2", "D" }, actual);

			// Test with space after a delimiter.
			actual = [.. TextUtility.SplitIntoTokens("A, B, \"C1, C2\"   , D ")];
			CollectionAssert.AreEqual(new[] { "A", "B", "C1, C2", "D" }, actual);

			// Test with leading and trailing empty tokens and with space before and after a delimited token.
			actual = [.. TextUtility.SplitIntoTokens(", B, \" C \" ,", ',', '"', false)];
			CollectionAssert.AreEqual(new[] { "", " B", "  C  ", "" }, actual);

			// Test with all empty tokens (where the two middle tokens get trimmed).
			actual = [.. TextUtility.SplitIntoTokens(", , ,")];
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);

			// The second token doesn't start with '"', so we treat it as a simple non-delimited token.
			actual = [.. TextUtility.SplitIntoTokens("A, B \"Test\", C")];
			CollectionAssert.AreEqual(new[] { "A", "B \"Test\"", "C" }, actual);

			// The second token is malformed because of the B after "Test".
			actual = [.. TextUtility.SplitIntoTokens("A, \"Test\"B, C")];
			CollectionAssert.AreEqual(new[] { "A", "TestB", "C" }, actual);
		}

		[TestMethod]
		public void SplitIntoTokensCollectionTest()
		{
			List<string> tokens = [];
			bool result = TextUtility.SplitIntoTokens("A,B,\"C,D\",E", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C,D", "E" }, tokens.ToArray());

			// This case has an unclosed last token, so SplitIntoTokens should return false.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("A,B,\"C,D", ',', '"', false, tokens);
			Assert.IsFalse(result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C,D" }, tokens.ToArray());

			// Test a quoted token with doubled/escaped quotes (and separators).
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("X,\"Y,\"\"y\"\"\",Z", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "X", "Y,\"y\"", "Z" }, tokens.ToArray());

			// Test where the last token is quoted.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("\" A \",\"B\"", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { " A ", "B" }, tokens.ToArray());

			// Test where the first and last tokens are quoted but the middle one isn't.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("'A',B,'C'", ',', '\'', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, tokens.ToArray());

			// Test where the last token is empty.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens(",,A,", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "", "", "A", "" }, tokens.ToArray());

			// Test where the last token is a space.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens(", ,A, ", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "", " ", "A", " " }, tokens.ToArray());

			// Test a single, plain token
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("X", ',', '"', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "X" }, tokens.ToArray());

			// Test a single, quoted token
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("'X'", ',', '\'', false, tokens);
			Assert.IsTrue(result);
			CollectionAssert.AreEqual(new[] { "X" }, tokens.ToArray());
		}

		[TestMethod()]
		public void StripQuotesTest()
		{
			string actual = TextUtility.StripQuotes("\"Test Case\"");
			actual.ShouldBe("Test Case");
			actual = TextUtility.StripQuotes("'Test Case'", "'");
			actual.ShouldBe("Test Case");
			actual = TextUtility.StripQuotes("[Test Case]", "[", "]");
			actual.ShouldBe("Test Case");
			actual = TextUtility.StripQuotes("Non-quoted", "[", "]");
			actual.ShouldBe("Non-quoted");
		}

		[DataRow("Application", "Applications")]
		[DataRow("Extension", "Extensions")]
		[DataRow("File", "Files")]
		[DataRow("Document", "Documents")]
		[DataRow("Library", "Libraries")]
		[DataRow("Folder", "Folders")]
		[DataRow("Driver", "Drivers")]
		[DataRow("Script", "Scripts")]
		[DataRow("Definition", "Definitions")]
		[DataRow("Source", "Sources")]
		[DataRow("License", "Licenses")]
		[DataRow("Cat", "Cats")]
		[DataRow("Cats", "Cats")]
		[DataRow("Deer", "Deer")]
		[DataRow("Tooth", "Teeth")]
		[DataRow("Matrix", "Matrices")]
		[DataRow("Compass", "Compasses")]
		[DataRow("Church", "Churches")]
		[DataRow("Bee", "Bees")]
		[DataRow("Man", "Men")]
		[DataRow("Woman", "Women")]
		[DataRow("Toy", "Toys")]
		[DataRow("Cherry", "Cherries")]
		[TestMethod]
		public void MakePluralTest(string mixedWord, string mixedExpected)
		{
			string mixedActual = TextUtility.MakePlural(mixedWord);
			mixedActual.ShouldBe(mixedExpected);

			string lowerActual = TextUtility.MakePlural(mixedWord.ToLower());
			lowerActual.ShouldBe(mixedExpected.ToLower());

			string upperActual = TextUtility.MakePlural(mixedWord.ToUpper());
			upperActual.ShouldBe(mixedExpected.ToUpper());
		}
	}
}
