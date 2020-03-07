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
		public void ExecuteTest()
		{
			static EditScript Diff(string left, string right, bool ignoreCase, bool ignoreOuterWhiteSpace, bool supportChangeEditType = true)
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

			static void Check(EditScript edits, params EditType[] expectedEditTypes)
			{
				edits.Count.ShouldEqual(expectedEditTypes.Length, nameof(edits.Count));
				for (int i = 0; i < expectedEditTypes.Length; i++)
				{
					Edit edit = edits[i];
					edit.EditType.ShouldEqual(expectedEditTypes[i]);
					edit.Length.ShouldEqual(1);
				}
			}

			string a = "\tA\nB\nC\t";
			string b = "\tA\nB\nD\t";
			string c = "\tA\nB\nc\t\nD\t";

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
	}
}