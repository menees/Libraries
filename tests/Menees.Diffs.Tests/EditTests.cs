using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class EditTests
	{
		// Note: All Edit properties are tested in TextDiffTests.
		[TestMethod]
		public void OffsetTest()
		{
			EditScript edits = TextDiffTests.Diff("1", "2", false, false);
			TextDiffTests.Check(edits, EditType.Change);
			Edit edit = edits[0];
			TextDiffTests.Check(edit, 1, 0, 0);

			edit.Offset(10, 20);
			edit.StartA.ShouldBe(10);
			edit.StartB.ShouldBe(20);

			edit.Offset(20, 10);
			edit.StartA.ShouldBe(30);
			edit.StartB.ShouldBe(30);
		}
	}
}