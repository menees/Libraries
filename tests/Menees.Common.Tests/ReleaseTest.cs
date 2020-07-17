using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Menees.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ReleaseTest
	{
		[TestMethod]
		public void FindGithubLatestTest()
		{
			Release libraries = Release.FindGithubLatest("Libraries");
			libraries.ShouldNotBeNull("Libraries Version");
			Version commonVersion = ReflectionUtility.GetVersion(typeof(Release).Assembly);
			libraries.Version.CompareTo(commonVersion).ShouldBeLessThanOrEqualTo(0, "Libraries Version <= Local Common Version");

			StringBuilder sb = new StringBuilder();
			bool canUpdate = libraries.CheckForUpdate(new Version("4.8.0"), null, msg => sb.Append(msg));
			canUpdate.ShouldBeTrue("Can update from 4.8.0");
			sb.ToString().ShouldContain("has been available since");

			sb.Clear();
			canUpdate = libraries.CheckForUpdate(new Version("100.0.0"), null, msg => sb.Append(msg));
			canUpdate.ShouldBeFalse("Can't update from 100.0.0");
			sb.ToString().ShouldContain("You're up-to-date");
		}

		[TestMethod]
		public void StaticCheckForUpdateTest()
		{
			StringBuilder sb = new StringBuilder();
			bool? check = Release.CheckForUpdate(typeof(Release).Assembly, null, msg => sb.Append(msg), "D28D424F-479C-456C-BA0B-E503545273EC");
			check.ShouldBeNull("Should not find non-existent repository.");
			sb.ToString().ShouldContain("Unable to determine");
		}
	}
}
