using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SoftwareApproach.TestingExtensions;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class AddCopyCollectionTests
	{
		[TestMethod]
		public void GDiffTest()
		{
			AddCopyCollection ac = BinaryDiffTests.Diff("Creative", "Creating", footprintLength: 2);
			BinaryDiffTests.Check(ac, false, true);
			using MemoryStream memory = new MemoryStream();
			ac.GDiff(memory);
			byte[] gdiff = memory.ToArray();

			// It always starts with "d1ff d1ff 4" (magic numbers and version).
			Check(gdiff, 0, 0xd1, 0xff, 0xd1, 0xff, 0x04);

			// Copy 6 bytes ("Creati").
			Check(gdiff, 5, 249, 0, 0, 6);

			// Add 2 bytes ("ng") and then EOF.
			Check(gdiff, 9, 2, (int)'n', (int)'g', 0);
		}

		private static void Check(byte[] diff, int startIndex, params byte[] expected)
		{
			diff.Length.ShouldBeGreaterThanOrEqualTo(startIndex + expected.Length);
			for (int i = 0; i < expected.Length; i++)
			{
				((int)diff[i + startIndex]).ShouldEqual(expected[i]);
			}
		}
	}
}
