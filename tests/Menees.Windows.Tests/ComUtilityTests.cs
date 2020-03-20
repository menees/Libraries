using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Menees.Windows.Tests
{
	[TestClass()]
	public class ComUtilityTests
	{
		[TestMethod()]
		public void CreateAndReleaseTest()
		{
			// WshShortcut Object Properties and Methods: http://msdn.microsoft.com/en-us/library/f5y78918.aspx
			dynamic shell = ComUtility.CreateInstance("WScript.Shell");
			try
			{
				var link = shell.CreateShortcut("TestShortcut.lnk");
				try
				{
					link.TargetPath = ApplicationInfo.ExecutableFile;
					link.Arguments = "/Argument=1";
					link.Description = "Testing a shortcut";
					// Don't save the link for this test: link.Save();
				}
				finally
				{
					ComUtility.FinalRelease(link);
				}
			}
			finally
			{
				ComUtility.FinalRelease(shell);
			}
		}
	}
}