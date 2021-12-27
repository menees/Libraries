using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ReflectionUtilityTest
	{
		[TestMethod]
		public void GetCopyrightTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string actual = ReflectionUtility.GetCopyright(asm);
			actual.ShouldEqual("Copyright For Unit Test");
		}

		[TestMethod]
		public void GetVersionTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			Version actual = ReflectionUtility.GetVersion(asm);
			actual.ShouldEqual(Version.Parse("1.2.3.0"));

			actual = ReflectionUtility.GetVersion(typeof(ReflectionUtility).Assembly);
			actual.CompareTo(Version.Parse("4.9.10")).ShouldBeGreaterThanOrEqualTo(0, "Common Version >= 4.9.10");
		}

		[TestMethod]
		public void GetBuildTimeTest()
		{
			// The Menees.Common.Tests assembly is built with a hardcoded build time.
			Assembly assembly = Assembly.GetExecutingAssembly();
			DateTime? built = ReflectionUtility.GetBuildTime(assembly);
			built.ShouldNotBeNull("Menees.Common.Tests assembly");
			built.ShouldEqual(DateTime.Parse("2021-12-26 17:50:00Z"));

			// The Menees.Common assembly is built normally so it will NOT have a build time.
			built = ReflectionUtility.GetBuildTime(typeof(Conditions).Assembly);
			built.ShouldBeNull("Menees.Common assembly");
		}
	}
}
