using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Shouldly;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class MyersDiffTests
	{
		[TestMethod]
		public void ExecuteTest()
		{
			// Execute is tested a lot via TextDiffTests (using MyersDiff<int> for string/line hashes).
			var diff = Diff("abc", "aoc");
			EditScript edits = diff.Execute();
			TextDiffTests.Check(edits, EditType.Change);
			TextDiffTests.Check(edits[0], 1, 1, 1);
		}

		[TestMethod]
		public void GetLongestCommonSubsequenceTest()
		{
			static string Lcs(string a, string b)
			{
				var diff = Diff(a, b);
				IList<char> sequence = diff.GetLongestCommonSubsequence();
				string result = new(sequence.ToArray());
				return result;
			}

			Lcs("abcdef", "abc").ShouldBe("abc");
			Lcs("abcdef", "def").ShouldBe("def");
			Lcs("abcdef", "abdeg").ShouldBe("abde");
			Lcs("abc", "def").ShouldBe("");
		}

		[TestMethod]
		public void GetLongestCommonSubsequenceLengthTest()
		{
			static int LcsLen(string a, string b)
			{
				var diff = Diff(a, b);
				int result = diff.GetLongestCommonSubsequenceLength();
				return result;
			}

			LcsLen("abcdef", "abc").ShouldBe(3);
			LcsLen("abcdef", "def").ShouldBe(3);
			LcsLen("abcdef", "abdeg").ShouldBe(4);
			LcsLen("abc", "def").ShouldBe(0);
		}

		[TestMethod]
		public void GetShortestEditScriptLengthTest()
		{
			static int SesLen(string a, string b)
			{
				var diff = Diff(a, b);
				int result = diff.GetShortestEditScriptLength();
				int reverseResult = diff.GetReverseShortestEditScriptLength();
				result.ShouldBe(reverseResult);
				return result;
			}

			SesLen("abcdef", "abc").ShouldBe(3);
			SesLen("abcdef", "def").ShouldBe(3);
			SesLen("abcd", "abcdef").ShouldBe(2);
			SesLen("abcdef", "abdeg").ShouldBe(3);
			SesLen("abc", "def").ShouldBe(6);
		}

		[TestMethod]
		public void GetSimilarityTest()
		{
			static int SimilarityPercent(string a, string b)
			{
				var diff = Diff(a, b);
				double similarity = diff.GetSimilarity();
				int result = (int)Math.Round(similarity * 100);
				return result;
			}

			SimilarityPercent("abc", "abc").ShouldBe(100);
			SimilarityPercent("abcdef", "abc").ShouldBe(67);
			SimilarityPercent("abcdef", "def").ShouldBe(67);
			SimilarityPercent("abcd", "abcdef").ShouldBe(80);
			SimilarityPercent("abcdef", "abdeg").ShouldBe(73);
			SimilarityPercent("abc", "def").ShouldBe(0);
		}

		private static MyersDiff<char> Diff(string a, string b) => new(a.ToCharArray(), b.ToCharArray(), true);
	}
}