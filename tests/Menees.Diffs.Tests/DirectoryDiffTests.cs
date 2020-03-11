using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;
using System.Diagnostics;
using System.IO;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class DirectoryDiffTests
	{
		private static DirectoryInfo DataDir { get; set; }

		private static DirectoryInfo ADir { get; set; }

		private static DirectoryInfo BDir { get; set; }

		private static DirectoryDiffFileFilter Filter { get; set; }

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			context.ShouldNotBeNull();

			string rootDir = context.TestRunDirectory;
			rootDir.ShouldNotBeEmpty();

			DataDir = new DirectoryInfo(Path.Combine(rootDir, "Data"));
			DataDir.Create();

			ADir = new DirectoryInfo(Path.Combine(DataDir.FullName, "A"));
			ADir.Create();

			BDir = new DirectoryInfo(Path.Combine(DataDir.FullName, "B"));
			BDir.Create();

			Filter = new DirectoryDiffFileFilter("*.txt", true);

			static void Write(DirectoryInfo directory, string fileName, string content)
				=> File.WriteAllText(Path.Combine(directory.FullName, fileName), content);

			Write(ADir, "1.txt", "One");
			Write(BDir, "1.txt", "One");

			Write(ADir, "2.txt", "Two");
			Write(BDir, "3.txt", "Three");

			Write(ADir, "4.txt", "Four");
			Write(BDir, "4.txt", "Four");

			Write(ADir, "5.txt", "Five");
			Write(BDir, "5.txt", "5");
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (DataDir.Exists)
			{
				DataDir.Delete(true);
			}
		}

		static void Check(DirectoryDiffEntry entry, string expectedName, bool expectInA, bool expectInB)
		{
			entry.Name.ShouldEqual(expectedName);
			entry.IsFile.ShouldBeTrue("Is File");
			entry.InA.ShouldEqual(expectInA, "In A");
			entry.InB.ShouldEqual(expectInB, "In B");
		}

		[TestMethod]
		public void ExecuteShowOnlyInATest()
		{
			// The first pass will use DirectoryDiff.DefaultNameComparison, which is OrdinalIgnoreCase on Windows.
			// The second pass will always use an Ordinal comparison.
			for (int pass = 1; pass <= 2; pass++)
			{
				DirectoryDiff diff = pass == 1
					? new DirectoryDiff(true, false, false, false, false, false, Filter)
					: new DirectoryDiff(true, false, false, false, false, false, Filter, StringComparison.Ordinal);
				DirectoryDiffResults results = diff.Execute(ADir, BDir);
				results.Entries.Count.ShouldEqual(1);
				Check(results.Entries[0], "2.txt", true, false);
			}
		}

		[TestMethod]
		public void ExecuteShowOnlyInBTest()
		{
			for (int pass = 1; pass <= 2; pass++)
			{
				DirectoryDiff diff = pass == 1
					? new DirectoryDiff(false, true, false, false, false, false, Filter)
					: new DirectoryDiff(false, true, false, false, false, false, Filter, StringComparison.Ordinal);
				DirectoryDiffResults results = diff.Execute(ADir, BDir);
				results.Entries.Count.ShouldEqual(1);
				Check(results.Entries[0], "3.txt", false, true);
			}
		}

		[TestMethod]
		public void ExecuteShowDifferentTest()
		{
			for (int pass = 1; pass <= 2; pass++)
			{
				DirectoryDiff diff = pass == 1
					? new DirectoryDiff(false, false, true, false, false, false, Filter)
					: new DirectoryDiff(false, false, true, false, false, false, Filter, StringComparison.Ordinal);
				DirectoryDiffResults results = diff.Execute(ADir, BDir);
				results.Entries.Count.ShouldEqual(1);
				Check(results.Entries[0], "5.txt", true, true);
			}
		}

		[TestMethod]
		public void ExecuteShowSameTest()
		{
			for (int pass = 1; pass <= 2; pass++)
			{
				DirectoryDiff diff = pass == 1
					? new DirectoryDiff(false, false, false, true, false, false, Filter)
					: new DirectoryDiff(false, false, false, true, false, false, Filter, StringComparison.Ordinal);
				DirectoryDiffResults results = diff.Execute(ADir, BDir);
				results.Entries.Count.ShouldEqual(2);
				Check(results.Entries[0], "1.txt", true, true);
				Check(results.Entries[1], "4.txt", true, true);
			}
		}

		[TestMethod]
		public void ExecuteShowNotSameTest()
		{
			for (int pass = 1; pass <= 2; pass++)
			{
				DirectoryDiff diff = pass == 1
					? new DirectoryDiff(true, true, true, false, false, false, Filter)
					: new DirectoryDiff(true, true, true, false, false, false, Filter, StringComparison.Ordinal);
				DirectoryDiffResults results = diff.Execute(ADir, BDir);
				results.Entries.Count.ShouldEqual(3);
				Check(results.Entries[0], "2.txt", true, false);
				Check(results.Entries[1], "3.txt", false, true);
				Check(results.Entries[2], "5.txt", true, true);
			}
		}
	}
}