using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ReflectionUtilityTest
	{
		[TestMethod]
		public void GetCopyrightTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string? actual = ReflectionUtility.GetCopyright(asm);
			actual.ShouldBe("Copyright For Unit Test");
		}

		[TestMethod]
		public void GetVersionTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			Version? actual = ReflectionUtility.GetVersion(asm);
			actual.ShouldBe(Version.Parse("1.2.3.0"));

			actual = ReflectionUtility.GetVersion(typeof(ReflectionUtility).Assembly);
			actual.ShouldNotBeNull();
			actual.CompareTo(Version.Parse("4.9.10")).ShouldBeGreaterThanOrEqualTo(0, "Common Version >= 4.9.10");
		}

		[TestMethod]
		public void GetBuildTimeTest()
		{
			// The Menees.Common.Tests assembly is built with a hardcoded build time.
			Assembly assembly = Assembly.GetExecutingAssembly();
			DateTime? built = ReflectionUtility.GetBuildTime(assembly);
			built.ShouldNotBeNull("Menees.Common.Tests assembly");
			DateTime expected = DateTime.Parse("2021-12-26 17:50:00Z", null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
			built.ShouldBe(expected);

			// The Menees.Common assembly is built with a date in debug builds and a datetime in release builds.
			built = ReflectionUtility.GetBuildTime(typeof(Conditions).Assembly);
			built.ShouldNotBeNull("Menees.Common assembly");
			if (ApplicationInfo.IsDebugBuild)
			{
				built.Value.TimeOfDay.ShouldBe(TimeSpan.Zero);
			}
		}

		[TestMethod]
		public void GetProductUrlTest()
		{
			Uri? url = ReflectionUtility.GetProductUrl(typeof(ReflectionUtility).Assembly);
			url.ShouldBeNull();

			url = ReflectionUtility.GetProductUrl(this.GetType().Assembly);
			url.ShouldBe(new Uri("http://www.menees.com"));
		}

		[TestMethod]
		public void IsDebugBuildTest()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			bool expected;
#if DEBUG
			expected = true;
#else
			expected = false;
#endif

			ReflectionUtility.IsDebugBuild(assembly).ShouldBe(expected);
		}
	}
}
