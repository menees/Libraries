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
		[TestMethod]
		public void OpenFileTest()
		{
			TestOpenFile();
		}

		private void TestOpenFile([CallerFilePath] string sourceFilePath = null)
		{
			if (!string.IsNullOrEmpty(sourceFilePath) && File.Exists(sourceFilePath))
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
