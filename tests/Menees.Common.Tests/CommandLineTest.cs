using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;
using System.Diagnostics;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class CommandLineTest
	{
		[TestMethod()]
		public void ConstructorTest()
		{
			CommandLine target = new();
			Assert.AreEqual(Environment.UserInteractive, target.UseConsole);

			target = new CommandLine(false);
			Assert.IsFalse(target.UseConsole);

			target = new CommandLine(false, StringComparer.CurrentCulture);
			Assert.IsFalse(target.UseConsole);
			Assert.AreEqual(StringComparer.CurrentCulture, target.Comparer);
		}

		[TestMethod()]
		public void AddHeaderTest()
		{
			CommandLine target = new(false);
			string[] lines = new[] { "First line", "Second line" };

			CommandLine actual = target.AddHeader(lines);
			Assert.AreEqual(target, actual);

			string output = GetMessage(target, CommandLineWriteOptions.Header);
			Assert.AreEqual(string.Join(Environment.NewLine, lines)+Environment.NewLine, output);
		}

		[TestMethod()]
		public void ArgumentsTest()
		{
			Debug.WriteLine(ApplicationInfo.ExecutableFile);
			Debug.WriteLine(Environment.CommandLine);
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				Debug.WriteLine(arg);
			}

			string[] actual = CommandLine.Arguments.ToArray();

			// In VS 2015 and .NET 4.6, sometimes the unit test process is launched with a path containing
			// spaces but not surrounded by quotes.  The Environment.GetCommandLineArgs() method won't
			// parse that correctly, but our CommandLine class works around it internally.  So we have to
			// work around it here too.  (This only happens when NOT debugging a unit test!)
			int skipCount = 1;
			if (Environment.CommandLine.StartsWith(ApplicationInfo.ExecutableFile)
				&& ApplicationInfo.ExecutableFile.Contains(' '))
			{
				skipCount = Environment.GetCommandLineArgs().Length - actual.Length;
			}

			CollectionAssert.AreEqual(Environment.GetCommandLineArgs().Skip(skipCount).ToArray(), actual);
		}

		[TestMethod()]
		public void ExecutableFileNameTest()
		{
			Debug.WriteLine(ApplicationInfo.ExecutableFile);
			Debug.WriteLine(Environment.CommandLine);
			foreach (string arg in Environment.GetCommandLineArgs())
			{
				Debug.WriteLine(arg);
			}

			string expected = Path.GetFileName(ApplicationInfo.ExecutableFile);
			string actual = CommandLine.ExecutableFileName;
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void CreateMessageTest()
		{
			CommandLine target = new(false);
			target.AddHeader("Header line");

			string expected = GetMessage(target, CommandLineWriteOptions.Header);
			string actual = target.CreateMessage();
			Assert.AreEqual(expected, actual);
		}

		private static string GetMessage(CommandLine target, CommandLineWriteOptions? options = null)
		{
			using (StringWriter writer = new())
			{
				if (options == null)
				{
					target.WriteMessage(writer);
				}
				else
				{
					target.WriteMessage(writer, options.Value);
				}
				string output = writer.ToString();
				return output;
			}
		}

		private sealed class TestData
		{
			public bool isBinary;
			public bool prompt;
			public bool verify;
			public string source;
			public List<string> targets = new();
		}

		private CommandLine CreateTester(TestData data)
		{
			CommandLine result = new(false);

			result.AddHeader("Copies a source file to one or more target locations.");
			result.AddHeader(string.Format("Usage: {0} [/B] [/P] [/V] source /T:target [/T:...]", CommandLine.ExecutableFileName));

			result.AddValueHandler(
				(value, errors) =>
				{
					if (!string.IsNullOrEmpty(data.source))
					{
						errors.Add("Only a single source file can be specified.");
					}
					else
					{
						data.source = value;
					}
				},
				new KeyValuePair<string, string>("Source", "The source file to copy."));

			result.AddSwitch("Binary", "Indicates a binary file (or a text file if the flag is disabled).\r\nUse /b+ to indicate binary or /b- to indicate text.  Defaults to text.", flag => data.isBinary = flag);
			result.AddSwitch("Prompt", "Prompts to confirm you want to overwrite an existing destination file.", flag => data.prompt = flag);
			result.AddSwitch(
				"Target",
				@"A target location to copy the source into.  Multiple target locations can be specified.  This can be a folder or a file (e.g., C:\Projects\CSharp\DesktopBackgroundChanger\CurrentRelease\DesktopBackgroundChanger.exe).",
				(value, errors) =>
				{
					data.targets.Add(value);
				},
				CommandLineSwitchOptions.Required | CommandLineSwitchOptions.AllowMultiple);
			result.AddSwitch("Verify", "Verifies that new files are written correctly.", flag => data.verify = flag);

			result.AddFinalValidation(errors =>
			{
				// Because we have a required, unnamed arg (source), we have to manually check for it.
				if (string.IsNullOrEmpty(data.source))
				{
					errors.Add("A source file is required.");
				}
			});

			return result;
		}

		[TestMethod()]
		public void ParseInvalidTest()
		{
			TestData data = new();
			CommandLine cmdLine = CreateTester(data);
			CommandLineParseResult result = cmdLine.Parse(new[] { "/Binary" });
			Assert.AreEqual(CommandLineParseResult.Invalid, result);
			string message = GetMessage(cmdLine);
			Assert.IsNotNull(message);
			Assert.IsTrue(message.Contains("A source file is required."), "Contains 'A source file is required.'");
			Assert.IsTrue(message.Contains("/Target"), "Contains '/Target'");
		}

		[TestMethod]
		public void ParseValidTest()
		{
			TestData data = new();
			CommandLine cmdLine = CreateTester(data);
			CommandLineParseResult result = cmdLine.Parse(new[] { "/Prompt", "/V", "/bin", @"C:\Input.txt", 
				@"/t:D:\ColonSeparated.txt", @"/Target", @"E:\SpaceSeparated.txt", @"/Targ=F:\EqualSeparated.txt" });
			Assert.AreEqual(CommandLineParseResult.Valid, result);
			Assert.AreEqual(true, data.prompt);
			Assert.AreEqual(true, data.verify);
			Assert.AreEqual(true, data.isBinary);
			Assert.AreEqual(@"C:\Input.txt", data.source);
			Assert.IsTrue(data.targets.Contains(@"D:\ColonSeparated.txt"), "Contains ColonSeparated");
			Assert.IsTrue(data.targets.Contains(@"E:\SpaceSeparated.txt"), "Contains SpaceSeparated");
			Assert.IsTrue(data.targets.Contains(@"F:\EqualSeparated.txt"), "Contains EqualSeparated");
		}

		[TestMethod]
		public void ParseHelpTest()
		{
			TestData data = new();
			CommandLine cmdLine = CreateTester(data);
			CommandLineParseResult result = cmdLine.Parse(new[] { "/?" });
			Assert.AreEqual(CommandLineParseResult.HelpRequested, result);
			string message = GetMessage(cmdLine);
			Assert.IsNotNull(message);
			Assert.IsTrue(message.StartsWith("Copies a source file to one or more target locations."), "Starts with 'Copies...'");
			Assert.IsTrue(message.EndsWith(Environment.NewLine), "Ends with newline.");
		}

		[TestMethod]
		public void ParseStaticTest()
		{
			List<string> values = new();
			Dictionary<string, string> switches = new();
			CommandLine.Parse(new[] { "a", "/b=c d" }, values, switches);
			values.Count.ShouldEqual(1);
			values[0].ShouldEqual("a");
			switches.Count.ShouldEqual(1);
			switches["b"].ShouldEqual("c d");
		}

		[TestMethod]
		public void SplitTest()
		{
			string commandLine = Environment.CommandLine;

			var hasProgramName = CommandLine.Split(commandLine, true);
			var missingProgramName = CommandLine.Split(commandLine, false);
			hasProgramName.Count().ShouldEqual(missingProgramName.Count() + 1);
			CollectionAssert.AreEqual(missingProgramName.ToArray(), hasProgramName.Skip(1).ToArray());

			commandLine = "Testing.exe /name:\"Application Testing\" /category:Test /win \"Extra Arg\"";

			hasProgramName = CommandLine.Split(commandLine, true);
			missingProgramName = CommandLine.Split(commandLine, false);
			CollectionAssert.AreEqual(missingProgramName.ToArray(), hasProgramName.Skip(1).ToArray());
			string[] args = hasProgramName.ToArray();
			args.Length.ShouldEqual(5);
			args[0].ShouldEqual("Testing.exe");
			args[1].ShouldEqual("/name:Application Testing");
			args[2].ShouldEqual("/category:Test");
			args[3].ShouldEqual("/win");
			args[4].ShouldEqual("Extra Arg");

			// These cases came from "Parsing C++ Command-Line Arguments"
			// http://msdn.microsoft.com/en-us/library/17w5ykft.aspx
			// Note: Win32's CommandLineToArgvW always treats the first argument
			// as a file name, so it doesn't allow escapes or double quotes within it.
			VerifySplit(@"test.exe ""abc"" d e", new[] { @"abc", "d", "e" });
			VerifySplit(@"test.exe a\\b d""e f""g h", new[] { @"a\\b", "de fg", "h" });
			VerifySplit(@"test.exe a\\\""b c d", new[] { @"a\""b", "c", "d" });
			VerifySplit(@"test.exe a\\\\""b c"" d e", new[] { @"a\\b c", "d", "e" });
		}

		private void VerifySplit(string commandLine, string[] expected)
		{
			string[] actual = CommandLine.Split(commandLine, false).ToArray();
			actual.Length.ShouldEqual(expected.Length);
			for (int i = 0; i < actual.Length; i++)
			{
				actual[i].ShouldEqual(expected[i], commandLine);
			}
		}

		[TestMethod]
		public void EncodeValueTest()
		{
			CommandLine.EncodeValue(null).ShouldEqual(null);
			CommandLine.EncodeValue(string.Empty).ShouldEqual(string.Empty);
			CommandLine.EncodeValue(" ").ShouldEqual("\" \"");
			CommandLine.EncodeValue("test").ShouldEqual("test");
			CommandLine.EncodeValue("a b").ShouldEqual("\"a b\"");
			CommandLine.EncodeValue("a\b").ShouldEqual("a\b");

			string actual = CommandLine.EncodeValue("she said, \"you had me at hello\"");
			actual.ShouldEqual(@"""she said, \""you had me at hello\""""");

			actual = CommandLine.EncodeValue(@"\some\directory with\spaces\");
			actual.ShouldEqual(@"""\some\directory with\spaces\\""");
		}

		[TestMethod]
		public void EncodeSwitchTest()
		{
			CommandLine.EncodeSwitch("a", null).ShouldEqual("/a");
			CommandLine.EncodeSwitch("a b", 123).ShouldEqual("/\"a b\"=123");
			CommandLine.EncodeSwitch("name", "has spaces").ShouldEqual("/name=\"has spaces\"");
			CommandLine.EncodeSwitch("-x", @"c:\temp").ShouldEqual(@"-x=c:\temp");
		}

		[TestMethod]
		public void BuildTest()
		{
			CommandLine.Build("abc", "d", "e").ShouldEqual("abc d e");
			CommandLine.Build(@"a\\b", "de fg").ShouldEqual(@"a\\b ""de fg""");
			CommandLine.Build(@"a\""b", "c", "d").ShouldEqual(@"""a\\\""b"" c d");
			CommandLine.Build(@"a\\b c", "d", "e").ShouldEqual(@"""a\\b c"" d e");

			string actual = CommandLine.Build("child.exe", "arg1", "arg 2", @"\some\path with\spaces");
			actual.ShouldEqual("child.exe arg1 \"arg 2\" \"\\some\\path with\\spaces\"");

			actual = CommandLine.Build("child.exe", "arg1", "she said, \"you had me at hello\"", @"\some\path with\spaces");
			actual.ShouldEqual("child.exe arg1 \"she said, \\\"you had me at hello\\\"\" \"\\some\\path with\\spaces\"");

			actual = CommandLine.Build("child.exe", @"\some\directory with\spaces\", "arg2");
			actual.ShouldEqual("child.exe \"\\some\\directory with\\spaces\\\\\" arg2");

			actual = CommandLine.Build("child.exe", "arg1", "arg\"2", "arg3");
			actual.ShouldEqual("child.exe arg1 \"arg\\\"2\" arg3");

			actual = CommandLine.Build("child.exe", new KeyValuePair<string, object>("x", @"C:\Temp\Test.txt"), Tuple.Create("y", (object)123));
			actual.ShouldEqual(@"child.exe /x=C:\Temp\Test.txt /y=123");

			actual = CommandLine.Build("child.exe", new KeyValuePair<string, object>("x", @"C:\A B\Test.txt"), new Tuple<string, object>("y", "C D"));
			actual.ShouldEqual(@"child.exe /x=""C:\A B\Test.txt"" /y=""C D""");
		}

		[TestMethod]
		public void MaxLengthTest()
		{
			int actual = CommandLine.MaxLength;

			// Windows 7 returns version 6.1.
			if (Environment.OSVersion.Version >= new Version(6, 1))
			{
				// On Windows 7 and later, command line length must be less than 37699.
				actual.ShouldEqual(32698);
			}
			else
			{
				// On Vista and earlier, command line length must be less than 2080.
				actual.ShouldEqual(2079);
			}
		}
	}
}
