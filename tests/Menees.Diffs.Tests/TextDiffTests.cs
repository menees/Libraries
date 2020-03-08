using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class TextDiffTests
	{
		[TestMethod]
		public void ExecuteGeneralTest()
		{
			string a = "\tA\nB\nC\t";
			string b = "\tA\nB\nD\t";
			string c = "\tA\nB\nc\t\nD\t";

			static void Check(EditScript edits, params EditType[] expectedEditTypes)
			{
				TextDiffTests.Check(edits, expectedEditTypes);
				foreach (Edit edit in edits)
				{
					edit.Length.ShouldEqual(1);
				}
			}

			Check(Diff(a, a, false, false));
			Check(Diff(a, a.ToLower(), true, false));
			Check(Diff(a, a.ToLower().Trim(), true, true));

			Check(Diff(a, b, false, false), EditType.Change);
			Check(Diff(a, b.ToLower(), true, false), EditType.Change);
			Check(Diff(a, b.ToLower(), true, false, false), EditType.Delete, EditType.Insert);

			Check(Diff(a, c, false, false), EditType.Change, EditType.Insert);
			Check(Diff(a, c, true, false), EditType.Insert);
			Check(Diff(b, c, true, false), EditType.Insert);
		}

		[TestMethod]
		public void ExecuteDeleteTest()
		{
			string a = "1\n2\n3\n4";
			string b = "1\n4";

			EditScript edits = Diff(a, b, false, false, false);
			Check(edits, EditType.Delete);
			Check(edits[0], 2, 1, 1);
		}

		[TestMethod]
		public void ExecuteInsertTest()
		{
			string a = "1\n4";
			string b = "1\n2\n3\n4";

			EditScript edits = Diff(a, b, false, false, false);
			Check(edits, EditType.Insert);
			Check(edits[0], 2, 1, 1);
		}

		[TestMethod]
		public void ExecuteChangeTest()
		{
			string a = "1\n2\n3\n4";
			string b = "1\nx\ny\n4";

			EditScript edits = Diff(a, b, false, false, true);
			Check(edits, EditType.Change);
			Check(edits[0], 2, 1, 1);
		}

		[TestMethod]
		public void ExecuteInsertDeleteTest()
		{
			string a = "1\n2\n3\n4";
			string b = "1\n3\n2\n4";

			EditScript edits = Diff(a, b, false, false, false);
			Check(edits, EditType.Insert, EditType.Delete);
			Check(edits[0], 1, 1, 1);
			Check(edits[1], 1, 2, 3);
		}

		[TestMethod]
		public void ExecuteDeleteInsertTest()
		{
			string a = "1\n2\n3";
			string b = "1\nx\n3";

			EditScript edits = Diff(a, b, false, false, false);
			Check(edits, EditType.Delete, EditType.Insert);
			Check(edits[0], 1, 1, 1);
			Check(edits[1], 1, 1, 1);
		}

		internal static EditScript Diff(string left, string right, bool ignoreCase, bool ignoreOuterWhiteSpace, bool supportChangeEditType = true)
		{
			IList<string> leftLines = DiffUtility.GetStringTextLines(left);
			IList<string> rightLines = DiffUtility.GetStringTextLines(right);

			EditScript edits = null;
			foreach (HashType hashType in Enum.GetValues(typeof(HashType)))
			{
				TextDiff diff = new TextDiff(hashType, ignoreCase, ignoreOuterWhiteSpace, 0, supportChangeEditType);
				EditScript newEdits = diff.Execute(leftLines, rightLines);
				if (edits != null)
				{
					edits.Count.ShouldEqual(newEdits.Count);
					edits.TotalEditLength.ShouldEqual(newEdits.TotalEditLength);
					edits.Similarity.ShouldEqual(newEdits.Similarity);
				}

				edits = newEdits;
			}

			return edits;
		}

		internal static void Check(EditScript edits, params EditType[] expectedEditTypes)
		{
			edits.Count.ShouldEqual(expectedEditTypes.Length, nameof(edits.Count));
			for (int i = 0; i < expectedEditTypes.Length; i++)
			{
				Edit edit = edits[i];
				edit.EditType.ShouldEqual(expectedEditTypes[i]);
			}
		}

		internal static void Check(Edit edit, int expectedLength, int expectedStartA, int expectedStartB)
		{
			edit.Length.ShouldEqual(expectedLength);
			edit.StartA.ShouldEqual(expectedStartA);
			edit.StartB.ShouldEqual(expectedStartB);
		}
	}
}