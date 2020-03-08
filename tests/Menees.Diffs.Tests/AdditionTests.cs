using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class AdditionTests
	{
		[TestMethod]
		public void GetBytesTest()
		{
			AddCopyCollection ac = BinaryDiffTests.Diff("A", "BC");
			BinaryDiffTests.Check(ac, true);
			Addition add = (Addition)ac[0];
			Encoding.UTF8.GetString(add.GetBytes()).ShouldEqual("BC");
		}
	}
}