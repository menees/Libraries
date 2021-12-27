using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Shouldly;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class BinaryDiffTests
	{
		[TestMethod]
		public void ExecuteTest()
		{
			// A word in the middle changed, so this should be Copy, Add, Copy.
			AddCopyCollection ac = Diff("This is the first item in the sequence.", "This is the second item in the sequence.");
			Check(ac, false, true, false);

			// These are nothing alike, so the version file is all Copy.
			ac = Diff("Student", "Handbook");
			Check(ac, true);

			// These two cases show how footprint length affects the diff.
			ac = Diff("Creatine pills", "Creative pills", footprintLength: 4);
			Check(ac, false, true, false); // Copy "Creati", Add "v", Copy "e pills".
			ac = Diff("Creatine pills", "Creative pills", footprintLength: 7);
			Check(ac, true, false); // Add "Creativ", Copy "e pills".
		}

		internal static AddCopyCollection Diff(
			string aContent,
			string bContent,
			int footprintLength = 8,
			int tableSize = 97,
			bool favorLastMatch = false)
		{
			BinaryDiff diff = new();
			diff.FavorLastMatch = favorLastMatch;
			diff.FootprintLength = footprintLength;
			diff.TableSize = tableSize;

			using Stream aStream = new MemoryStream(Encoding.UTF8.GetBytes(aContent));
			using Stream bStream = new MemoryStream(Encoding.UTF8.GetBytes(bContent));
			AddCopyCollection result = diff.Execute(aStream, bStream);

			result.TotalByteLength.ShouldBe((int)bStream.Length);
			return result;
		}

		internal static void Check(AddCopyCollection ac, params bool[] adds)
		{
			ac.Count.ShouldBe(adds.Length);
			for (int i = 0; i < adds.Length; i++)
			{
				IAddCopy addCopy = ac[i];
				bool isAdd = addCopy.IsAdd;
				isAdd.ShouldBe(adds[i]);
				if (addCopy is Addition add)
				{
					add.IsAdd.ShouldBeTrue();
					add.GetBytes().Length.ShouldBeGreaterThan(0);
				}
				else
				{
					Copy copy = (Copy)addCopy;
					copy.IsAdd.ShouldBeFalse();
					copy.Length.ShouldBeGreaterThan(0);
					if (i == 0)
					{
						copy.BaseOffset.ShouldBe(0);
					}
					else
					{
						copy.BaseOffset.ShouldBeGreaterThan(0);
					}
				}
			}
		}
	}
}