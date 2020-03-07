using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;
using System.IO;
using System.Linq;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class DiffUtilityTests
	{
		[TestMethod]
		public void AreFilesDifferentTest()
		{
			using TempFile a1 = new TempFile("1234");
			using TempFile a2 = new TempFile("1234");
			using TempFile b = new TempFile("12345");
			using TempFile c = new TempFile("12045");

			static void Test(TempFile f1, TempFile f2, bool expected, string message)
			{
				DiffUtility.AreFilesDifferent(f1.FileName, f2.FileName).ShouldEqual(expected, message);
				DiffUtility.AreFilesDifferent(f1.Info, f2.Info).ShouldEqual(expected, message);
			}

			Test(a1, a2, false, "a1==a2");
			Test(a1, b, true, "a1!=b");
			Test(a2, c, true, "a2!=c");
			Test(b, c, true, "b!=c");
		}

		[TestMethod]
		public void GetFileTextLinesTest()
		{
			using TempFile a = new TempFile("Line1");
			using TempFile b = new TempFile("Line1\nLine2");
			using TempFile c = new TempFile("Line1\r\nLine2\r\n");
			using TempFile d = new TempFile("Line1\r\nLine2\r\nLine3");

			static void TestLines(TempFile f, params string[] expected)
			{
				IList<string> actual = DiffUtility.GetFileTextLines(f.FileName);
				CollectionAssert.AreEquivalent(expected, actual.ToArray());
			}

			TestLines(a, "Line1");
			TestLines(b, "Line1", "Line2");
			TestLines(c, "Line1", "Line2");
			TestLines(d, "Line1", "Line2", "Line3");
		}

		[TestMethod]
		public void GetStringTextLinesTest()
		{
			static void TestLines(string text, params string[] expected)
			{
				IList<string> actual = DiffUtility.GetStringTextLines(text);
				CollectionAssert.AreEquivalent(expected, actual.ToArray());
			}

			TestLines("");
			TestLines("1", "1");
			TestLines("1\r\n2", "1", "2");
			TestLines("1\r\n2\r\n3", "1", "2", "3");
		}

		[TestMethod]
		public void GetTextLinesTest()
		{
			// The GetTextLines function is internally used by GetFileTextLines and GetStringTextLines, so other tests cover it too.
			using TempFile f = new TempFile("Line1\r\nLine2\r\nLine3");
			using TextReader reader = new StreamReader(f.FileName);
			IList<string> actual = DiffUtility.GetTextLines(reader);
			CollectionAssert.AreEquivalent(new[] { "Line1", "Line2", "Line3" }, actual.ToArray());
		}

		[TestMethod]
		public void GetXmlTextLinesTest()
		{
			throw new NotImplementedException(); // TODO: Write test. [2/29/20]
		}

		[TestMethod]
		public void GetXmlTextLinesFromXmlTest()
		{
			throw new NotImplementedException(); // TODO: Write test. [2/29/20]
		}

		[TestMethod]
		public void IsBinaryFileTest()
		{
			using (TempFile t = new TempFile("This is text."))
			{
				DiffUtility.IsBinaryFile(t.FileName).ShouldBeFalse("text filename");
				using (Stream s = File.OpenRead(t.FileName))
				{
					DiffUtility.IsBinaryFile(s).ShouldBeFalse("text stream");
				}
			}

			using (TempFile t = new TempFile("Has\0embedded\0nulls."))
			{
				DiffUtility.IsBinaryFile(t.FileName).ShouldBeTrue("binary filename");
				using (Stream s = File.OpenRead(t.FileName))
				{
					DiffUtility.IsBinaryFile(s).ShouldBeTrue("binary stream");
				}
			}
		}
	}
}