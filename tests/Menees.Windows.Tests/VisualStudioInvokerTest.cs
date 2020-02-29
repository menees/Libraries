using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Windows.Tests
{
	[TestClass]
	public class VisualStudioInvokerTest
	{
		// Change the parameter to 1 to make this test do something.
		// It's off by default because I don't want every test run messing with VS's active file.
		private static readonly bool AffectUI = Convert.ToBoolean(0);

		[TestMethod]
		public void OpenFileTest()
		{
			TestOpenFile();
		}

		private void TestOpenFile([CallerFilePath] string sourceFilePath = null)
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

					bool opened = VisualStudioInvoker.OpenFile(sourceFilePath, "1");
					opened.ShouldBeTrue("Opened in DevEnv");
				}
			}
		}
	}
}
