using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Menees.Common.Tests
{
	[TestClass]
	public class UriUtilityTest
	{
		[TestMethod]
		public void AppendToPathTest()
		{
			Uri baseUri = new("ftp://testing");
			Uri actual = UriUtility.AppendToPath(baseUri, string.Empty);
			actual.ShouldBe(baseUri);

			actual = UriUtility.AppendToPath(baseUri, "Folder1");
			actual.ShouldBe(new Uri(baseUri.ToString() + "Folder1"));

			actual = UriUtility.AppendToPath(baseUri, "/Folder1");
			actual.ShouldBe(new Uri(baseUri.ToString() + "Folder1"));

			actual = UriUtility.AppendToPath(baseUri, "File.txt");
			actual.ShouldBe(new Uri(baseUri.ToString() + "File.txt"));

			// Test using a file path that ends with '\'.  In URIs, '\' should get converted to '/'.
			baseUri = new Uri(@"C:\");
			string expectedBase = "file:///C:/";
			actual = UriUtility.AppendToPath(baseUri, string.Empty);
			actual.ToString().ShouldBe(expectedBase);

			actual = UriUtility.AppendToPath(baseUri, "Folder1");
			actual.ShouldBe(new Uri(expectedBase + "Folder1"));

			// The base ends with '/' and the new part starts with '/'.
			actual = UriUtility.AppendToPath(baseUri, "/Folder1");
			actual.ShouldBe(new Uri(expectedBase + "Folder1"));

			actual = UriUtility.AppendToPath(baseUri, "File.txt");
			actual.ShouldBe(new Uri(expectedBase + "File.txt"));

			// Make sure backslashes are normalized to forward slashes correctly.
			actual = UriUtility.AppendToPath(baseUri, @"PathA\PathB");
			actual.ShouldBe(new Uri(expectedBase + "PathA/PathB"));

			// Make sure the leading backslash is handled correctly.
			actual = UriUtility.AppendToPath(baseUri, @"\PathA\PathB\");
			actual.ShouldBe(new Uri(expectedBase + "PathA/PathB/"));
		}
	}
}
