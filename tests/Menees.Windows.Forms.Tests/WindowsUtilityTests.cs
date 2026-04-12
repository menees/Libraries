namespace Menees.Windows.Forms.Tests;

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

[TestClass]
public class WindowsUtilityTests
{
	[TestMethod]
	public void FindWindowsTerminalTest()
	{
		// Windows Terminal may not be available on the GitHub build agent.
		// See https://stackoverflow.com/a/68006153/1882616 for info on wt.exe.
		string? actual = WindowsUtility.FindWindowsTerminal();
		string expected = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Microsoft\WindowsApps\wt.exe");
		if (File.Exists(expected))
		{
			actual.ShouldBe(expected, StringCompareShould.IgnoreCase);
		}
		else
		{
			actual.ShouldBeNull();
		}
	}
}
