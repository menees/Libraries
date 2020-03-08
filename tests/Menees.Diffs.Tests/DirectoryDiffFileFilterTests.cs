using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;
using System.IO;
using System.Linq;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class DirectoryDiffFileFilterTests
	{
		[TestMethod]
		public void DirectoryDiffFileFilterTest()
		{
			var filter = new DirectoryDiffFileFilter("*.txt;*.xml", true);
			filter.Filters.ShouldEqual("*.txt;*.xml");
			filter.Include.ShouldBeTrue();

			filter = new DirectoryDiffFileFilter("*.bin", false);
			filter.Filters.ShouldEqual("*.bin");
			filter.Include.ShouldBeFalse();
		}

		[TestMethod]
		public void FilterTest()
		{
			using TempFile a = new TempFile("A");
			using TempFile b = new TempFile("B");

			string directory = Path.GetDirectoryName(a.FileName);
			directory.ShouldEqual(Path.GetDirectoryName(b.FileName));
			DirectoryInfo info = new DirectoryInfo(directory);

			string filterString = Path.GetFileName(a.FileName) + ";" + Path.GetFileName(b.FileName);
			var include = new DirectoryDiffFileFilter(filterString, true);
			FileInfo[] files = include.Filter(info);
			if (files[0].FullName != a.FileName)
			{
				Array.Reverse(files);
			}

			files[0].FullName.ShouldEqual(a.FileName);
			files[1].FullName.ShouldEqual(b.FileName);

			var exclude = new DirectoryDiffFileFilter(filterString, false);
			files = exclude.Filter(info);
			files.Any(f => f.FullName == a.FileName || f.FullName == b.FileName).ShouldBeFalse();
		}
	}
}