using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using SoftwareApproach.TestingExtensions;
using System.Collections.Generic;
using System.Globalization;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class FileUtilityTest
	{
		// http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx
		private const int ERROR_FILE_NOT_FOUND = 2;
		private const int ERROR_ACCESS_DENIED = 5;

		private static string CreateTempFile()
		{
			string result = FileUtility.GetTempFileName(".txt");
			File.WriteAllText(result, "Test file.");
			return result;
		}

		[TestMethod]
		public void DeleteFileTest()
		{
			string fileName = CreateTempFile();

			// Test it on an existing file.
			FileUtility.DeleteFile(fileName);

			// Test it when the drive and folder exist but the file doesn't.
			FileUtility.DeleteFile(fileName);

			// Test it when the drive doesn't exist.
			FileUtility.DeleteFile(@"B:\Test.txt");

			// Test is when the folder doesn't exist.
			FileUtility.DeleteFile(@"C:\" + Guid.NewGuid().ToString("N") + @"\Test.txt");
		}

		[TestMethod()]
		public void TryDeleteFileTest()
		{
			string fileName = CreateTempFile();

			bool actual = FileUtility.TryDeleteFile(fileName);
			Assert.AreEqual(true, actual);

			// It shouldn't be there now.
			actual = FileUtility.TryDeleteFile(fileName);
			Assert.AreEqual(false, actual);
		}

		[TestMethod()]
		public void TryDeleteFileErrorCodeTest()
		{
			string fileName = CreateTempFile();

			bool actual = FileUtility.TryDeleteFile(fileName, out int errorCode);
			Assert.AreEqual(true, actual);
			Assert.AreEqual(0, errorCode);

			// It shouldn't be there now.
			actual = FileUtility.TryDeleteFile(fileName, out errorCode);
			Assert.AreEqual(false, actual);
			Assert.AreEqual(ERROR_FILE_NOT_FOUND, errorCode);
		}

		[TestMethod()]
		public void GetTempFileNameTest()
		{
			string extension = ".tmp";
			string actual = FileUtility.GetTempFileName(extension);
			Assert.AreEqual(extension, Path.GetExtension(actual));

			string directory = Path.GetDirectoryName(actual);
			string tempDir = Path.GetTempPath();
			if (!string.IsNullOrEmpty(tempDir) && tempDir.EndsWith(@"\"))
			{
				tempDir = tempDir.Substring(0, tempDir.Length-1);
			}
			Assert.AreEqual(tempDir, directory);
		}

		[TestMethod()]
		public void GetTempFileNameDirectoryTest()
		{
			string extension = ".txt";
			string directory = @"C:\";
			string actual = FileUtility.GetTempFileName(extension, directory);
			Assert.AreEqual(extension, Path.GetExtension(actual));
			Assert.AreEqual(directory, Path.GetDirectoryName(actual));
		}

		[TestMethod()]
		public void IsReadOnlyFileTest()
		{
			string fileName = CreateTempFile();
			bool actual = FileUtility.IsReadOnlyFile(fileName);
			Assert.AreEqual(false, actual);

			File.SetAttributes(fileName, FileAttributes.ReadOnly);
			actual = FileUtility.IsReadOnlyFile(fileName);
			Assert.AreEqual(true, actual);

			// Try to delete it while it is read-only, so we can test DeleteFile too.
			actual = FileUtility.TryDeleteFile(fileName, out int errorCode);
			Assert.AreEqual(false, actual);
			Assert.AreEqual(ERROR_ACCESS_DENIED, errorCode);

			File.SetAttributes(fileName, FileAttributes.Normal);
			actual = FileUtility.IsReadOnlyFile(fileName);
			Assert.AreEqual(false, actual);

			FileUtility.TryDeleteFile(fileName);
		}

		[TestMethod]
		public void ExpandFileNameTest()
		{
			string fileName = "Sample.txt";
			string actual = FileUtility.ExpandFileName(fileName);
			Assert.AreEqual(Path.Combine(ApplicationInfo.BaseDirectory, fileName), actual);

			string tempfileName = @"%Temp%\" + fileName;
			actual = FileUtility.ExpandFileName(tempfileName);
			Assert.AreEqual(Path.Combine(Path.GetTempPath(), fileName), actual);

			string fixedfileName = @"C:\Test\" + fileName;
			actual = FileUtility.ExpandFileName(fixedfileName);
			Assert.AreEqual(fixedfileName, actual);
		}

		[TestMethod]
		public void TryGetExactPathNameTest()
		{
			string machineName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Environment.MachineName.ToLower());
			string[] testPaths = new[]
				{
					@"C:\Users\Public\desktop.ini",
					@"C:\pagefile.sys",
					@"C:\Windows\System32\cmd.exe",
					@"C:\Users\Default\NTUSER.DAT",
					@"C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies",
					@"C:\Program Files (x86)",
					@"Does not exist",
					@"\\Nas\Main\Setups",
					@"\\Nas\Main\Setups\Microsoft\Visual Studio\VS 2015\vssdk_full.exe",
					@"\\" + machineName + @"\C$\Windows\System32\ActionCenter.dll",
					@"..",
				};
			Dictionary<string, string> expectedExactPaths = new Dictionary<string, string>()
				{
					{ @"..", Path.GetDirectoryName(Environment.CurrentDirectory) },
				};

			foreach (string testPath in testPaths)
			{
				string lowercasePath = testPath.ToLower();
				bool expected = File.Exists(lowercasePath) || Directory.Exists(lowercasePath);
				bool actual = FileUtility.TryGetExactPath(lowercasePath, out string exactPath);
				actual.ShouldEqual(expected);
				if (actual)
				{
					if (expectedExactPaths.TryGetValue(testPath, out string expectedExactPath))
					{
						exactPath.ShouldEqual(expectedExactPath);
					}
					else
					{
						exactPath.ShouldEqual(testPath);
					}
				}
				else
				{
					exactPath.ShouldBeNull();
				}
			}
		}

		[TestMethod]
		public void IsValidNameTest()
		{
			Dictionary<string, bool> tests = new Dictionary<string, bool>()
			{
				{ @"Test", true },
				{ @"Test.txt", true },
				{ @"A and B", true },
				{ @"A and B.suffix", true },
				{ @"Test 123.exe.config", true },
				{ @".temp", true },
				{ @" Has Leading And Trailing Whitespace.txt ", true },

				{ @"..", false },
				{ @".", false },
				{ @"C:", false },
				{ @"\\", false },
				{ @"LPT1", false },
				{ @"AUX", false },
				{ @"CON.TXT", false },
				{ @"A < B", false },
				{ @"A | B.txt", false },
				{ @"A * B", false },
			};

			foreach (var pair in tests)
			{
				bool actual = FileUtility.IsValidName(pair.Key);
				actual.ShouldEqual(pair.Value, pair.Key);
			}
		}

		[TestMethod]
		public void IsValidPathTest()
		{
			Dictionary<string, ValidPathOptions> validPaths = new Dictionary<string,ValidPathOptions>()
			{
				{ @"\\.\PhysicalDisk1", ValidPathOptions.AllowDevicePaths },
				{ @"\\?\C:\Test" + new string('X', 300), ValidPathOptions.AllowLongPaths },
				{ @"\\?\C:\Test", ValidPathOptions.AllowLongPaths },
				{ @"\\1.dads\C", ValidPathOptions.None },
				{ @"\\be\projects$\Wield\Rff\", ValidPathOptions.AllowTrailingSeparator },
				{ @"\\Dads\Mp3\FileName1\.\TestDir2", ValidPathOptions.AllowRelative },
				{ @"\\Dpk\T c\", ValidPathOptions.AllowTrailingSeparator },
				{ @"\\machine1\c$\shared 4\hello world.txt", ValidPathOptions.None },
				{ @"\\machine1\shared_2\hello_world.txt", ValidPathOptions.None },
				{ @"\\mypath\mypath1\myfile.aaa", ValidPathOptions.None },
				{ @"\\otherpath\otherpath", ValidPathOptions.None },
				{ @"\\server", ValidPathOptions.AllowDevicePaths },
				{ @"\\server\directory with space", ValidPathOptions.None },
				{ @"\\server\directory\", ValidPathOptions.AllowTrailingSeparator },
				{ @"\\server\directory1\directory2\file1.xxx", ValidPathOptions.None },
				{ @"\\server\share$\subdir1 with space\subdir2", ValidPathOptions.None },
				{ @"\\server\share\directory", ValidPathOptions.None },
				{ @"\\server\share\subdir1\subdir2", ValidPathOptions.None },
				{ @"C:", ValidPathOptions.AllowDevicePaths },
				{ @"C:..\tmp.txt", ValidPathOptions.AllowRelative }, // Valid example per https://msdn.microsoft.com/en-us/library/aa365247.aspx
				{ @"C:\", ValidPathOptions.AllowTrailingSeparator },
				{ @"c:\34\445\546\3.htm", ValidPathOptions.None },
				{ @"c:\directory", ValidPathOptions.None },
				{ @"c:\directory\file.xx", ValidPathOptions.None },
				{ @"C:\mypath\mypath\mypath", ValidPathOptions.None },
				{ @"C:\Program Files\Test", ValidPathOptions.None },
				{ @"C:\random space\test.txt", ValidPathOptions.None },
				{ @"C:\test for spaces\New Text Document.txt", ValidPathOptions.AllowLongPaths | ValidPathOptions.AllowDevicePaths },
				{ @"C:\test_for_spaces\test_extension", ValidPathOptions.None },
				{ @"C:\test_for_spaces\test_extension.t92", ValidPathOptions.None },
				{ @"d:\", ValidPathOptions.AllowTrailingSeparator },
				{ @"D:\somepath\somefile.file", ValidPathOptions.None },
				{ @"E:\reference\h101\", ValidPathOptions.AllowTrailingSeparator },
				{ @"G:\GD", ValidPathOptions.None },
				{ @"\\server\share\directory\\file.xx.xx", ValidPathOptions.None },
				{ @"E:\reference\\\h101\//\", ValidPathOptions.AllowTrailingSeparator },
			};

			Dictionary<string, ValidPathOptions> invalidPaths = new Dictionary<string,ValidPathOptions>()
			{
				{ @".", ValidPathOptions.None },
				{ @"..", ValidPathOptions.None },
				{ @"..\", ValidPathOptions.None },
				{ @".ext", ValidPathOptions.None },
				{ @"\. folder\", ValidPathOptions.None },
				{ @"\\", ValidPathOptions.None },
				{ @"\\$erver\sh*re\directory", ValidPathOptions.None },
				{ @"\\.\PhysicalDisk1", ValidPathOptions.None },
				{ @"\\?\C:/Test/ForwardSlashesNotAllowed", ValidPathOptions.AllowLongPaths },
				{ @"\\?\C:\Test", ValidPathOptions.None },
				{ @"\\?\C:\Test\..\Test.txt", ValidPathOptions.AllowRelative | ValidPathOptions.AllowLongPaths },
				{ @"\\\badpath\badpath", ValidPathOptions.None },
				{ @"\\cae\..", ValidPathOptions.None },
				{ @"\\Dpk\", ValidPathOptions.None },
				{ @"\\machine1\c$\test|pipe:colon.txt", ValidPathOptions.None },
				{ @"\\qaz", ValidPathOptions.None },
				{ @"\\server\\directory\file.xx.xx", ValidPathOptions.None },
				{ @"\server\share", ValidPathOptions.None },
				{ @"a:\di:::r", ValidPathOptions.None },
				{ @"AUX", ValidPathOptions.None },
				{ @"b\c\..\x.", ValidPathOptions.None },
				{ @"C:", ValidPathOptions.None },
				{ @"c:?Program Files\Lab", ValidPathOptions.None },
				{ @"c:\\", ValidPathOptions.None },
				{ @"c:\Ram<\", ValidPathOptions.None },
				{ @"C\\bad\test.t", ValidPathOptions.None },
				{ @"CON.txt", ValidPathOptions.None },
				{ @"http://www.mysite.com", ValidPathOptions.None },
				{ @"j:ohn\", ValidPathOptions.None },
				{ @"LPT1", ValidPathOptions.None },
				{ @"RDT_PROJ_MK::{42D00E44-28B8-4CAA-950E-909D5273945D}", ValidPathOptions.None },
				{ @"This is a very 'long' folder\that is.part of 2 folders.", ValidPathOptions.None },
				{ @"X::\\badpath", ValidPathOptions.None },
			};

			TestIsValidPath(validPaths, true);
			TestIsValidPath(invalidPaths, false);
		}

		private void TestIsValidPath(Dictionary<string, ValidPathOptions> paths, bool expected)
		{
			foreach (var pair in paths.OrderBy(p => p.Key))
			{
				bool actual = FileUtility.IsValidPath(pair.Key, pair.Value);
				actual.ShouldEqual(expected, "{0} should be {1}.", pair.Key, expected ? "valid" : "invalid");
			}
		}
	}
}
