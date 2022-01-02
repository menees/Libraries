using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Shouldly;
using System.Linq;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class BinaryDiffLinesTests
	{
		[TestMethod]
		public void BinaryDiffLinesTest()
		{
			string baseText = "The first one";
			AddCopyCollection ac = BinaryDiffTests.Diff(baseText, "The second one", footprintLength: 4);
			BinaryDiffTests.Check(ac, false, true, false);

			using MemoryStream baseStream = new(Encoding.UTF8.GetBytes(baseText));
			BinaryDiffLines lines = new(baseStream, ac, 4);
			Check(lines.BaseLines,
				"00000000    54 68 65 20    The ",
				"00000004    66 69 72 73    firs",
				"00000008    74             t",
				"00000009    20 6F 6E 65     one");
			Check(lines.VersionLines,
				"00000000    54 68 65 20    The ",
				"00000004    73 65 63 6F    seco",
				"00000008    6E 64          nd",
				"0000000A    20 6F 6E 65     one");
		}

		private static void Check(IList<string> actualLines, params string[] expectedLines)
			=> CollectionAssert.AreEquivalent(expectedLines, actualLines.ToArray());
	}
}