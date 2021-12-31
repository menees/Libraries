using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ShellUtilityTest
	{
		[TestMethod]
		public void GetCopyrightInfoTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string? actual = ShellUtility.GetCopyrightInfo(asm);
			actual.ShouldBe("Copyright For Unit Test");

			actual = ShellUtility.GetCopyrightInfo(null);
			actual.ShouldBe(ShellUtility.GetCopyrightInfo(typeof(ShellUtility).Assembly));
		}

		[TestMethod]
		public void GetVersionInfoTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string actual = ShellUtility.GetVersionInfo(asm);
			DateTime? built = ReflectionUtility.GetBuildTime(asm);
			built.ShouldNotBeNull();

			// Runtime bitness check from https://stackoverflow.com/a/3782556/1882616.
			string expected = "Version 1.2.3 – " + built.Value.ToLocalTime().ToShortDateString() + " – " + 8 * IntPtr.Size + "-bit";

#if NETFRAMEWORK
			expected += " – Framework";
#elif NETCOREAPP
			expected += " – Core";
#endif

			if (ApplicationInfo.IsUserRunningAsAdministrator)
			{
				expected += " – Administrator";
			}

			actual.ShouldBe(expected);
		}

		[TestMethod]
		public void ShellExecuteTest()
		{
			using (Process? process = ShellUtility.ShellExecute(null, "Notepad.exe"))
			{
				if (process != null)
				{
					process.CloseMainWindow();
					process.Kill();
				}
			}
		}

		[TestMethod]
		public void GetFileTypeInfoTest()
		{
			// The shell functions and System.Drawing APIs may not work in services.
			if (Environment.UserInteractive)
			{
				string? typeName = ShellUtility.GetFileTypeInfo(".cs", false, IconOptions.None, null);
				typeName.ShouldNotBeNull();
				typeName.ShouldContain("C# Source File", Case.Insensitive);

				string notepadPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\Notepad.exe");
				Icon? icon = null;
				typeName = ShellUtility.GetFileTypeInfo(notepadPath, true, IconOptions.Large, hIcon => icon = CloneIcon(hIcon));
				typeName.ShouldBe("Application");
				icon.ShouldNotBeNull();
				icon.Size.ShouldBe(SystemInformation.IconSize);
				icon.Dispose();

				typeName = ShellUtility.GetFileTypeInfo(
					".dll",
					false,
					IconOptions.Small | IconOptions.Shortcut | IconOptions.Selected,
					hIcon => icon = CloneIcon(hIcon));
				typeName.ShouldBe("Application extension");
				icon.Size.ShouldBe(SystemInformation.SmallIconSize);
				icon.Dispose();
			}
		}

		private static Icon CloneIcon(IntPtr hIcon) => (Icon)Icon.FromHandle(hIcon).Clone();
	}
}
