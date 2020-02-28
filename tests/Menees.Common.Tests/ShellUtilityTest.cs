using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class ShellUtilityTest
	{
		[TestMethod]
		public void GetCopyrightInfoTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string actual = ShellUtility.GetCopyrightInfo(asm);
			actual.ShouldEqual("Copyright For Unit Test");

			actual = ShellUtility.GetCopyrightInfo(null);
			actual.ShouldEqual(ShellUtility.GetCopyrightInfo(typeof(ShellUtility).Assembly));
		}

		[TestMethod]
		public void GetVersionInfoTest()
		{
			Assembly asm = Assembly.GetExecutingAssembly();
			string actual = ShellUtility.GetVersionInfo(asm);
			DateTime? built = ReflectionUtility.GetBuildTime(asm);

			// Runtime bitness check from https://stackoverflow.com/a/3782556/1882616.
			actual.ShouldEqual("Version 1.2.3 – " + built.Value.ToLocalTime().ToShortDateString() + " – " + 8*IntPtr.Size + "-bit");
		}

		[TestMethod]
		public void ShellExecuteTest()
		{
			using (Process process = ShellUtility.ShellExecute(null, "Notepad.exe"))
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
				string typeName = ShellUtility.GetFileTypeInfo(".cs", false, IconOptions.None, null);
				typeName.ShouldEqual("Visual C# Source File");

				string notepadPath = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\Notepad.exe");
				Icon icon = null;
				typeName = ShellUtility.GetFileTypeInfo(notepadPath, true, IconOptions.Large, hIcon => icon = CloneIcon(hIcon));
				typeName.ShouldEqual("Application");
				icon.Size.ShouldEqual(SystemInformation.IconSize);
				icon.Dispose();

				typeName = ShellUtility.GetFileTypeInfo(
					".dll",
					false,
					IconOptions.Small | IconOptions.Shortcut | IconOptions.Selected,
					hIcon => icon = CloneIcon(hIcon));
				typeName.ShouldEqual("Application extension");
				icon.Size.ShouldEqual(SystemInformation.SmallIconSize);
				icon.Dispose();
			}
		}

		private static Icon CloneIcon(IntPtr hIcon) => (Icon)Icon.FromHandle(hIcon).Clone();
	}
}
