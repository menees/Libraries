using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Menees.Common.Tests
{


	[TestClass()]
	public class ConsoleOutputBufferTest
	{
		private static readonly TimeSpan defaultWaitTime = TimeSpan.MaxValue;

		private static ConsoleOutputBuffer Create(TimeSpan waitTime)
		{
			// We have to execute a DIR command on a directory that has something in it.
			// If we execute a DIR command that matches no entries, then it will return
			// exit code 1, which would fail some of our unit tests.
			ProcessStartInfo startInfo = new("cmd.exe", @"/c dir C:\");
			ConsoleOutputBuffer result = new(startInfo, true, waitTime);
			return result;
		}

		[TestMethod()]
		public void GetLinesTest()
		{
			ConsoleOutputBuffer target = Create(defaultWaitTime);
			string[] actual = target.GetLines();
			Assert.IsTrue(actual.Length > 0, "Number of lines > 0");

			// Sometimes there will be an extra blank line at the beginning of the DIR output.
			string? firstNonEmptyLine = null;
			foreach (string line in actual)
			{
				Trace.WriteLine(line);
				if (firstNonEmptyLine == null && !string.IsNullOrEmpty(line))
				{
					firstNonEmptyLine = line;
				}
			}

			Assert.IsTrue(firstNonEmptyLine != null && firstNonEmptyLine.StartsWith(" Volume in drive "), "Starts with ' Volume in drive '");
		}

		[TestMethod()]
		public void GetTextTest()
		{
			ConsoleOutputBuffer target = Create(defaultWaitTime);
			string actual = target.GetText();
			Assert.IsTrue(!string.IsNullOrEmpty(actual), "Text is non-empty.");
			Assert.IsTrue(actual.StartsWith(" Volume in drive "), "Starts with ' Volume in drive '");
			Assert.IsTrue(actual.Contains(" Directory of "), "Contains ' Directory of '");
		}

		[TestMethod()]
		public void ProcessNotExitedTest()
		{
			// Wait zero time, so we might get HasProcessExited as false.
			ConsoleOutputBuffer target = Create(TimeSpan.Zero);
			bool actual = target.HasProcessExited;
			// On a fast system, the process may finish "instantly".
			Assert.IsTrue(!actual || (actual && target.ProcessExitCode == 0), "Process has not exited.");
		}

		[TestMethod()]
		public void ProcessExitedTest()
		{
			ConsoleOutputBuffer target = Create(defaultWaitTime);
			Assert.AreEqual(0, target.ProcessExitCode);
			Assert.AreEqual(true, target.HasProcessExited);
		}
	}
}
