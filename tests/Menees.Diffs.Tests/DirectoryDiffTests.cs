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
	[DeploymentItem("Data", "Data")]
	public class DirectoryDiffTests
	{
		public TestContext TestContext { get; set; }

		private DirectoryInfo ADir { get; set; }

		private DirectoryInfo BDir { get; set; }

		private DirectoryDiffFileFilter Filter { get; set; }

		[TestInitialize]
		public void TestInitialize()
		{
			// The unit testing framework will set this property.
			this.TestContext.ShouldNotBeNull();

			string rootDir = this.TestContext.DeploymentDirectory;
			rootDir.ShouldNotBeEmpty();

			string dataDir = Path.Combine(rootDir, "Data");
			Directory.Exists(dataDir).ShouldBeTrue("Data");

			this.ADir = new DirectoryInfo(Path.Combine(dataDir, "A"));
			this.ADir.Exists.ShouldBeTrue("ADir");
			this.BDir = new DirectoryInfo(Path.Combine(dataDir, "B"));
			this.BDir.Exists.ShouldBeTrue("BDir");

			this.Filter = new DirectoryDiffFileFilter("*.txt", true);
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
			DirectoryDiff diff = new DirectoryDiff(true, false, false, false, false, false, this.Filter);
			DirectoryDiffResults results = diff.Execute(this.ADir, this.BDir);
			results.Entries.Count.ShouldEqual(1);
			Check(results.Entries[0], "2.txt", true, false);
		}

		[TestMethod]
		public void ExecuteShowOnlyInBTest()
		{
			DirectoryDiff diff = new DirectoryDiff(false, true, false, false, false, false, this.Filter);
			DirectoryDiffResults results = diff.Execute(this.ADir, this.BDir);
			results.Entries.Count.ShouldEqual(1);
			Check(results.Entries[0], "3.txt", false, true);
		}

		[TestMethod]
		public void ExecuteShowDifferentTest()
		{
			DirectoryDiff diff = new DirectoryDiff(false, false, true, false, false, false, this.Filter);
			DirectoryDiffResults results = diff.Execute(this.ADir, this.BDir);
			results.Entries.Count.ShouldEqual(1);
			Check(results.Entries[0], "5.txt", true, true);
		}

		[TestMethod]
		public void ExecuteShowSameTest()
		{
			DirectoryDiff diff = new DirectoryDiff(false, false, false, true, false, false, this.Filter);
			DirectoryDiffResults results = diff.Execute(this.ADir, this.BDir);
			results.Entries.Count.ShouldEqual(2);
			Check(results.Entries[0], "1.txt", true, true);
			Check(results.Entries[1], "4.txt", true, true);
		}

		[TestMethod]
		public void ExecuteShowNotSameTest()
		{
			DirectoryDiff diff = new DirectoryDiff(true, true, true, false, false, false, this.Filter);
			DirectoryDiffResults results = diff.Execute(this.ADir, this.BDir);
			results.Entries.Count.ShouldEqual(3);
			Check(results.Entries[0], "2.txt", true, false);
			Check(results.Entries[1], "3.txt", false, true);
			Check(results.Entries[2], "5.txt", true, true);
		}
	}
}