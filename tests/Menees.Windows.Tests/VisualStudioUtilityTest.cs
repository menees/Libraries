using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Windows.Tests
{
	[TestClass]
	public class VisualStudioUtilityTest
	{
		// Change the parameter to 1 to make this test do something.
		// It's off by default because I don't want every test run messing with VS's active file.
		private static readonly bool AffectUI = Convert.ToBoolean(0);

		[TestMethod]
		public void OpenFileTest()
		{
			// This has to call a method so the compiler can fill in the [CallerFilePath] and [CallerLineNumber] parameters.
			TestOpenFile();
		}

		private void TestOpenFile([CallerFilePath] string sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = 0)
		{
			if (AffectUI && !string.IsNullOrEmpty(sourceFilePath) && File.Exists(sourceFilePath))
			{
				Process[] processes = Process.GetProcessesByName("DevEnv");
				if (processes.Length > 0)
				{
					foreach (Process devEnv in processes)
					{
						devEnv.Dispose();
					}

					bool opened = VisualStudioUtility.OpenFile(sourceFilePath, sourceLineNumber.ToString());
					opened.ShouldBeTrue("Opened in DevEnv");
				}
			}
		}

		[TestMethod]
		public void ResolvePathVsTest()
		{
			int foundVersion = 0;
			int missingVersion = 0;

			// Try to make this unit test work for 20 years or so (assuming 2 year release cycles).
			for (int vsMajorVersion = 15; vsMajorVersion <= 25; vsMajorVersion++)
			{
				string vsExePath = VisualStudioUtility.ResolvePath(ver => @"Common7\IDE\DevEnv.exe", vsMajorVersion, vsMajorVersion);
				string vsComPath = VisualStudioUtility.ResolvePath(ver => @"Common7\IDE\DevEnv.com", vsMajorVersion, vsMajorVersion);
				if (File.Exists(vsExePath))
				{
					File.Exists(vsComPath).ShouldBeTrue("DevEnv.com should exist since DevEnv.exe exists.");
					foundVersion = vsMajorVersion;
				}
				else if (string.IsNullOrEmpty(vsExePath))
				{
					missingVersion = vsMajorVersion;
				}
			}

			string foundVersionPath = VisualStudioUtility.ResolvePath(ver => @"Common7\IDE\DoesNotExist.exe", foundVersion, foundVersion, true);
			foundVersionPath.ShouldBeNull($"Version {foundVersion} exists, but the requested path doesn't exist.");

			foundVersionPath = VisualStudioUtility.ResolvePath(ver => @"Common7\IDE\DoesNotExist.exe", foundVersion, foundVersion, false);
			foundVersionPath.ShouldNotBeNull($"Version {foundVersion} exists, and the path can be resolved (but doesn't exist).");

			string missingVersionPath = VisualStudioUtility.ResolvePath(ver => @"Common7\IDE\DevEnv.exe", missingVersion, missingVersion);
			missingVersionPath.ShouldBeNull($"Version {missingVersion} doesn't exist.");
		}

		[TestMethod]
		public void ResolvePathMsBuildTest()
		{
			string msBuildPath = VisualStudioUtility.ResolvePath(version =>
				{
					string versionPath = version.Major == VisualStudioUtility.VS2017MajorVersion ? $"{VisualStudioUtility.VS2017MajorVersion}.0" : "Current";
					return $@"MSBuild\{versionPath}\Bin\Roslyn";
				},
				VisualStudioUtility.VS2017MajorVersion,
				resolvedPathMustExist: true);

			msBuildPath.ShouldNotBeEmpty();
			Directory.Exists(msBuildPath).ShouldBeTrue("MSBuild's Roslyn directory must exist.");
		}
	}
}
