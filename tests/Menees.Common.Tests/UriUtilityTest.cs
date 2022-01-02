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
			Assert.AreEqual(baseUri, actual);

			actual = UriUtility.AppendToPath(baseUri, "Folder1");
			Assert.AreEqual(new Uri(baseUri.ToString() + "Folder1"), actual);

			actual = UriUtility.AppendToPath(baseUri, "/Folder1");
			Assert.AreEqual(new Uri(baseUri.ToString() + "Folder1"), actual);

			actual = UriUtility.AppendToPath(baseUri, "File.txt");
			Assert.AreEqual(new Uri(baseUri.ToString() + "File.txt"), actual);

			// Test using a file path that ends with '\'.  In URIs, '\' should get converted to '/'.
			baseUri = new Uri(@"C:\");
			string expectedBase = "file:///C:/";
			actual = UriUtility.AppendToPath(baseUri, string.Empty);
			Assert.AreEqual(expectedBase, actual.ToString());

			actual = UriUtility.AppendToPath(baseUri, "Folder1");
			Assert.AreEqual(new Uri(expectedBase + "Folder1"), actual);

			// The base ends with '/' and the new part starts with '/'.
			actual = UriUtility.AppendToPath(baseUri, "/Folder1");
			Assert.AreEqual(new Uri(expectedBase + "Folder1"), actual);

			actual = UriUtility.AppendToPath(baseUri, "File.txt");
			Assert.AreEqual(new Uri(expectedBase + "File.txt"), actual);

			// Make sure backslashes are normalized to forward slashes correctly.
			actual = UriUtility.AppendToPath(baseUri, @"PathA\PathB");
			Assert.AreEqual(new Uri(expectedBase + "PathA/PathB"), actual);

			// Make sure the leading backslash is handled correctly.
			actual = UriUtility.AppendToPath(baseUri, @"\PathA\PathB\");
			Assert.AreEqual(new Uri(expectedBase + "PathA/PathB/"), actual);
		}
	}
}
