using Microsoft.VisualStudio.TestTools.UnitTesting;
using Menees.Diffs;
using System;
using System.Collections.Generic;
using System.Text;
using SoftwareApproach.TestingExtensions;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Menees.Diffs.Tests
{
	[TestClass]
	public class DiffUtilityTests
	{
		[TestMethod]
		public void AreFilesDifferentTest()
		{
			static void Test(TempFile f1, TempFile f2, bool expected, string message)
			{
				DiffUtility.AreFilesDifferent(f1.FileName, f2.FileName).ShouldEqual(expected, message);
				DiffUtility.AreFilesDifferent(f1.Info, f2.Info).ShouldEqual(expected, message);
			}

			using TempFile a1 = new TempFile("1234");
			using TempFile a2 = new TempFile("1234");
			using TempFile b = new TempFile("12345");
			using TempFile c = new TempFile("12045");

			Test(a1, a2, false, "a1==a2");
			Test(a1, b, true, "a1!=b");
			Test(a2, c, true, "a2!=c");
			Test(b, c, true, "b!=c");
		}

		private static void AreEquivalent(IList<string> actual, params string[] expected)
		{
			CollectionAssert.AreEquivalent(expected, actual.ToArray());
		}

		[TestMethod]
		public void GetFileTextLinesTest()
		{
			static void TestLines(TempFile f, params string[] expected)
			{
				IList<string> actual = DiffUtility.GetFileTextLines(f.FileName);
				AreEquivalent(actual, expected);
			}

			using TempFile a = new TempFile("Line1");
			using TempFile b = new TempFile("Line1\nLine2");
			using TempFile c = new TempFile("Line1\r\nLine2\r\n");
			using TempFile d = new TempFile("Line1\r\nLine2\r\nLine3");

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
				AreEquivalent(actual, expected);
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
			AreEquivalent(actual, "Line1", "Line2", "Line3");
		}

		[TestMethod]
		public void GetXmlTextLinesTest()
		{
			XElement element = XElement.Parse("<a>\n\t<b>\n\n\t\t<c/>\n\t</b>\n</a>");
			using TempFile a = new TempFile("");
			element.Save(a.FileName);

			const string Header = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";

			// Ignoring insignificant whitespace still makes the parser skip the extra blank line after <b>, but it keeps our tabs as-is.
			AreEquivalent(DiffUtility.GetXmlTextLines(a.FileName, ignoreInsignificantWhiteSpace: true), Header, "<a>", "\t<b>", "\t\t<c />", "\t</b>", "</a>");

			// Not ignoring insignificant whitespace makes the parser skip the extra blank line and convert tabs to spaces.
			AreEquivalent(DiffUtility.GetXmlTextLines(a.FileName, ignoreInsignificantWhiteSpace: false), Header, "<a>", "  <b>", "    <c />", "  </b>", "</a>");
		}

		[TestMethod]
		public void GetXmlTextLinesFromXmlTest()
		{
			const string Xml = "<a>\n\t<b>\n\n\t\t<c/>\n\t</b>\n</a>";
			const string Header = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";

			// Ignoring insignificant whitespace still makes the parser skip the extra blank line after <b>, but it keeps our tabs as-is.
			AreEquivalent(DiffUtility.GetXmlTextLinesFromXml(Xml, ignoreInsignificantWhiteSpace: true), Header, "<a>", "\t<b>", "\t\t<c />", "\t</b>", "</a>");

			// Not ignoring insignificant whitespace makes the parser include the extra blank line.
			AreEquivalent(DiffUtility.GetXmlTextLinesFromXml(Xml, ignoreInsignificantWhiteSpace: false), Header, "<a>", "\t<b>", "", "\t\t<c />", "\t</b>", "</a>");
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