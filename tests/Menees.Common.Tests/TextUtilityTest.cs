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
			Assert.AreEqual('\u2190', actual); // Left arrow
			actual = TextUtility.GetPrintableCharacter('\n');
			Assert.AreEqual('\u2193', actual); // Down arrow
			actual = TextUtility.GetPrintableCharacter('\0');
			Assert.AreEqual('\u03D5', actual); // Phi/nil

			actual = TextUtility.GetPrintableCharacter('A');
			Assert.AreEqual('A', actual);
		}

		[TestMethod()]
		public void EnsureQuotesTest()
		{
			string actual = TextUtility.EnsureQuotes("Test");
			Assert.AreEqual("\"Test\"", actual);
			actual = TextUtility.EnsureQuotes("Test", "'");
			Assert.AreEqual("'Test'", actual);
			actual = TextUtility.EnsureQuotes("Test", "[", "]");
			Assert.AreEqual("[Test]", actual);
			actual = TextUtility.EnsureQuotes(actual, "[", "]");
			Assert.AreEqual("[Test]", actual);
		}

		[TestMethod()]
		public void ReplaceTest()
		{
			string actual = TextUtility.Replace("Testing tester TESTED tests.", "Test", "Record", StringComparison.CurrentCultureIgnoreCase);
			Assert.AreEqual("Recording Recorder RecordED Records.", actual);

			actual = TextUtility.Replace("Testing tester TESTED tests.", "Test", "Buy", StringComparison.CurrentCultureIgnoreCase);
			Assert.AreEqual("Buying Buyer BuyED Buys.", actual);

			string text = "This has no matches.";
			actual = TextUtility.Replace(text, "ABCD", "wxyz", StringComparison.OrdinalIgnoreCase);
			Assert.AreEqual(text, actual);
		}

		[TestMethod()]
		public void ReplaceControlCharactersTest()
		{
			string actual = TextUtility.ReplaceControlCharacters("Line one\r\nLine two\0", ' ');
			Assert.AreEqual("Line one  Line two ", actual);

			actual = TextUtility.ReplaceControlCharacters("Line one\r\nLine two\0");
			Assert.AreEqual("Line one\u2190\u2193Line two\u03D5", actual);
		}

		[TestMethod]
		public void SplitIntoTokensNoDelimiterTest()
		{
			string[] actual = TextUtility.SplitIntoTokens("A, B, C, D", ',', null, true).ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, actual);

			// The double quotes here should be part of the the token since we're not using a delimiter.
			actual = TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D", ',', null, true).ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "\"C1", "C2\"", "D" }, actual);

			// Do the same thing but don't trim the tokens.
			actual = TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D", ',', null, false).ToArray();
			CollectionAssert.AreEqual(new[] { "A", " B", " \"C1", " C2\"", " D" }, actual);

			// Test with leading and trailing empty tokens.
			actual = TextUtility.SplitIntoTokens(", , ,", ',', null, false).ToArray();
			CollectionAssert.AreEqual(new[] { "", " ", " ", "" }, actual);

			// Test with all empty tokens (where the two middle tokens get trimmed).
			actual = TextUtility.SplitIntoTokens(", , ,", ',', null, true).ToArray();
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);
		}

		[TestMethod]
		public void SplitIntoTokensListTest()
		{
			string[] actual = TextUtility.SplitIntoTokens("A, B, C").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, actual);

			actual = TextUtility.SplitIntoTokens(" A , B , C ").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, actual);

			actual = TextUtility.SplitIntoTokens(",, ,").ToArray();
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);

			actual = TextUtility.SplitIntoTokens("a=A; b=B;' c=C;See '; d=D", ';', '\'', true).ToArray();
			CollectionAssert.AreEqual(new[] { "a=A", "b=B", "c=C;See", "d=D" }, actual);

			// Test with space before a delimiter.
			actual = TextUtility.SplitIntoTokens("A, B, \"C1, C2\", D").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C1, C2", "D" }, actual);

			// Test with space after a delimiter.
			actual = TextUtility.SplitIntoTokens("A, B, \"C1, C2\"   , D ").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C1, C2", "D" }, actual);

			// Test with leading and trailing empty tokens and with space before and after a delimited token.
			actual = TextUtility.SplitIntoTokens(", B, \" C \" ,", ',', '"', false).ToArray();
			CollectionAssert.AreEqual(new[] { "", " B", "  C  ", "" }, actual);

			// Test with all empty tokens (where the two middle tokens get trimmed).
			actual = TextUtility.SplitIntoTokens(", , ,").ToArray();
			CollectionAssert.AreEqual(new[] { "", "", "", "" }, actual);

			// The second token doesn't start with '"', so we treat it as a simple non-delimited token.
			actual = TextUtility.SplitIntoTokens("A, B \"Test\", C").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B \"Test\"", "C" }, actual);

			// The second token is malformed because of the B after "Test".
			actual = TextUtility.SplitIntoTokens("A, \"Test\"B, C").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "TestB", "C" }, actual);
		}

		[TestMethod]
		public void SplitIntoTokensCollectionTest()
		{
			List<string> tokens = new();
			bool result = TextUtility.SplitIntoTokens("A,B,\"C,D\",E", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C,D", "E" }, tokens.ToArray());

			// This case has an unclosed last token, so SplitIntoTokens should return false.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("A,B,\"C,D", ',', '"', false, tokens);
			Assert.AreEqual(false, result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C,D" }, tokens.ToArray());

			// Test a quoted token with doubled/escaped quotes (and separators).
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("X,\"Y,\"\"y\"\"\",Z", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "X", "Y,\"y\"", "Z" }, tokens.ToArray());

			// Test where the last token is quoted.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("\" A \",\"B\"", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { " A ", "B" }, tokens.ToArray());

			// Test where the first and last tokens are quoted but the middle one isn't.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("'A',B,'C'", ',', '\'', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, tokens.ToArray());

			// Test where the last token is empty.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens(",,A,", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "", "", "A", "" }, tokens.ToArray());

			// Test where the last token is a space.
			tokens.Clear();
			result = TextUtility.SplitIntoTokens(", ,A, ", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "", " ", "A", " " }, tokens.ToArray());

			// Test a single, plain token
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("X", ',', '"', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "X" }, tokens.ToArray());

			// Test a single, quoted token
			tokens.Clear();
			result = TextUtility.SplitIntoTokens("'X'", ',', '\'', false, tokens);
			Assert.AreEqual(true, result);
			CollectionAssert.AreEqual(new[] { "X" }, tokens.ToArray());
		}

		[TestMethod()]
		public void StripQuotesTest()
		{
			string actual = TextUtility.StripQuotes("\"Test Case\"");
			Assert.AreEqual("Test Case", actual);
			actual = TextUtility.StripQuotes("'Test Case'", "'");
			Assert.AreEqual("Test Case", actual);
			actual = TextUtility.StripQuotes("[Test Case]", "[", "]");
			Assert.AreEqual("Test Case", actual);
			actual = TextUtility.StripQuotes("Non-quoted", "[", "]");
			Assert.AreEqual("Non-quoted", actual);
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
		[DataTestMethod]
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
