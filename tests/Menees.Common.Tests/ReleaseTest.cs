using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Menees.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ReleaseTest
	{
		[TestMethod]
		public void FindGithubLatestTest()
		{
			Release? libraries = Release.FindGithubLatest("Libraries");
			libraries.ShouldNotBeNull("Libraries Version");
			Version? commonVersion = ReflectionUtility.GetVersion(typeof(Release).Assembly);
			libraries.Version.CompareTo(commonVersion).ShouldBeLessThanOrEqualTo(0, "Libraries Version <= Local Common Version");
		}
	}
}
