using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SoftwareApproach.TestingExtensions;

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
				string result = new string(sequence.ToArray());
				return result;
			}

			Lcs("abcdef", "abc").ShouldEqual("abc");
			Lcs("abcdef", "def").ShouldEqual("def");
			Lcs("abcdef", "abdeg").ShouldEqual("abde");
			Lcs("abc", "def").ShouldEqual("");
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

			LcsLen("abcdef", "abc").ShouldEqual(3);
			LcsLen("abcdef", "def").ShouldEqual(3);
			LcsLen("abcdef", "abdeg").ShouldEqual(4);
			LcsLen("abc", "def").ShouldEqual(0);
		}

		[TestMethod]
		public void GetShortestEditScriptLengthTest()
		{
			static int SesLen(string a, string b)
			{
				var diff = Diff(a, b);
				int result = diff.GetShortestEditScriptLength();
				int reverseResult = diff.GetReverseShortestEditScriptLength();
				result.ShouldEqual(reverseResult);
				return result;
			}

			SesLen("abcdef", "abc").ShouldEqual(3);
			SesLen("abcdef", "def").ShouldEqual(3);
			SesLen("abcd", "abcdef").ShouldEqual(2);
			SesLen("abcdef", "abdeg").ShouldEqual(3);
			SesLen("abc", "def").ShouldEqual(6);
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

			SimilarityPercent("abc", "abc").ShouldEqual(100);
			SimilarityPercent("abcdef", "abc").ShouldEqual(67);
			SimilarityPercent("abcdef", "def").ShouldEqual(67);
			SimilarityPercent("abcd", "abcdef").ShouldEqual(80);
			SimilarityPercent("abcdef", "abdeg").ShouldEqual(73);
			SimilarityPercent("abc", "def").ShouldEqual(0);
		}

		private static MyersDiff<char> Diff(string a, string b) => new MyersDiff<char>(a.ToCharArray(), b.ToCharArray(), true);
	}
}