using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
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
			filter.Filters.ShouldBe("*.txt;*.xml");
			filter.Include.ShouldBeTrue();

			filter = new DirectoryDiffFileFilter("*.bin", false);
			filter.Filters.ShouldBe("*.bin");
			filter.Include.ShouldBeFalse();
		}

		[TestMethod]
		public void FilterTest()
		{
			using TempFile a = new("A");
			using TempFile b = new("B");

			string? directory = Path.GetDirectoryName(a.FileName);
			directory.ShouldNotBeNull();
			directory.ShouldBe(Path.GetDirectoryName(b.FileName));
			DirectoryInfo info = new(directory);
			string filterString = Path.GetFileName(a.FileName) + ";" + Path.GetFileName(b.FileName);

			// The first pass will use DirectoryDiff.DefaultNameComparison, which is OrdinalIgnoreCase on Windows.
			// The second pass will always use an Ordinal comparison.
			for (int pass = 1; pass <= 2; pass++)
			{
				var include = pass == 1
					? new DirectoryDiffFileFilter(filterString, true)
					: new DirectoryDiffFileFilter(filterString, true, StringComparison.Ordinal);

				FileInfo[] files = include.Filter(info);
				files[0].FullName.ShouldBe(a.FileName);
				files[1].FullName.ShouldBe(b.FileName);

				var exclude = pass == 1
					? new DirectoryDiffFileFilter(filterString, false)
					: new DirectoryDiffFileFilter(filterString, false, StringComparison.Ordinal);

				files = exclude.Filter(info);
				files.Any(f => f.FullName == a.FileName || f.FullName == b.FileName).ShouldBeFalse();
			}
		}
	}
}