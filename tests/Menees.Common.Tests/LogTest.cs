using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Menees.Diagnostics;
using System.Diagnostics;
using System.IO;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class LogTest
	{
		private StringWriter writer;
		private TextWriterTraceListener listener;

		public TestContext TestContext { get; set; }

		#region Helper Methods And Types

		[TestInitialize()]
		public void LogTestInitialize()
		{
			this.writer = new StringWriter();
			this.listener = new TextWriterTraceListener(this.writer, TestContext.TestName);
			GetLog().Listeners.Add(this.listener);

			Log.GlobalContext["Context Level"] = "Global Level Context";
			Log.ThreadContext["Context Level"] = "Thread Level Context";
		}

		[TestCleanup()]
		public void LogTestCleanup()
		{
			GetLog().Listeners.Remove(this.listener);
		}

		private static Log GetLog() => Log.GetLog(typeof(LogTest));

		private sealed class TestData
		{
			public TestData(string message)
			{
				this.message = message;
			}

			public string Message => this.message;

			public override string ToString() => this.message;

			private readonly string message;
		}

		private IDictionary<string, object> CreateTestProperties()
		{
			//I'm intentionally using a case-sensitive dictionary here.
			//Most places I use case-insensitive, so I want to test
			//with a mix of comparers.
			Dictionary<string, object> properties = new Dictionary<string, object>();
			properties.Add("abc", 123);
			properties.Add("Even Digits", new int[] { 2, 4, 6, 8 });
			properties.Add("Company", new TestData("Menees Software"));
			properties.Add("Context Level", "Message Level Context");
			return properties;
		}

		private void AssertLogEntry(object originalMessage, Type category, LogLevel level)
		{
			AssertLogEntry(originalMessage, category, level, null, null);
		}

		private void AssertLogEntry(object originalMessage, Type category, LogLevel level, Exception ex)
		{
			AssertLogEntry(originalMessage, category, level, ex, null);
		}

		private void AssertLogEntry(object originalMessage, Type category, LogLevel level, Exception ex, IDictionary<string, object> properties)
		{
			string categoryName = Log.GetLog(category).CategoryName;
			AssertLogEntry(originalMessage, categoryName, level, ex, properties);
		}

		private void AssertLogEntry(object originalMessage, string category, LogLevel level)
		{
			AssertLogEntry(originalMessage, category, level, null, null);
		}

		private void AssertLogEntry(object originalMessage, string category, LogLevel level, Exception ex)
		{
			AssertLogEntry(originalMessage, category, level, ex, null);
		}

		private void AssertLogEntry(object originalMessageData, string category, LogLevel level, Exception ex, IDictionary<string, object> properties)
		{
			string originalMessage = originalMessageData is TestData testData ? testData.Message : Convert.ToString(originalMessageData);
			this.listener.Flush();
			string finalMessage = this.writer.ToString();

			if (string.IsNullOrEmpty(finalMessage))
			{
				Assert.Fail("No messages were logged.");
			}

			if (!finalMessage.Contains(originalMessage))
			{
				Assert.Fail("The original message was not present in the final message.");
			}

			if (!finalMessage.Contains(category))
			{
				Assert.Fail("The category information was not logged: {0}", category);
			}

			TraceEventType eventType;
			switch (level)
			{
				case LogLevel.Debug:
					eventType = TraceEventType.Verbose;
					break;

				case LogLevel.Error:
					eventType = TraceEventType.Error;
					break;

				case LogLevel.Fatal:
					eventType = TraceEventType.Critical;
					break;

				case LogLevel.Info:
					eventType = TraceEventType.Information;
					break;

				case LogLevel.Warning:
					eventType = TraceEventType.Warning;
					break;

				default: // None
					// TraceEventType doesn't define a "None" field or a named 0-valued field,
					// so we have to refer to it using C#'s default keyword.
					eventType = default;
					break;
			}

			if (finalMessage.IndexOf(" " + eventType.ToString() + ": ") < 0)
			{
				Assert.Fail("The level information was not logged: {0}", level);
			}

			if (ex != null && !finalMessage.Contains(ex.Message))
			{
				Assert.Fail("The exception message was not present in the final message.");
			}

			if (!finalMessage.Contains("Context Level"))
			{
				Assert.Fail("The \"Context Level\" property was not present in the final message.");
			}

			if (properties != null)
			{
				foreach (var pair in properties)
				{
					if (!finalMessage.Contains(pair.Key))
					{
						Assert.Fail("The final message does not contain the key: " + pair.Key);
					}
				}
			}
		}

		#endregion

		[TestMethod()]
		public void DebugTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Debug(messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Debug);
		}

		[TestMethod()]
		public void DebugExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Debug(messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Debug, ex);
		}

		[TestMethod()]
		public void DebugExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Debug(messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Debug, ex, properties);
		}

		[TestMethod()]
		public void DebugStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Debug(category, messageData);
			AssertLogEntry(messageData, category, LogLevel.Debug);
		}

		[TestMethod()]
		public void DebugStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Debug(category, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Debug, ex);
		}

		[TestMethod()]
		public void DebugStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Debug(category, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Debug, ex, properties);
		}

		[TestMethod()]
		public void ErrorTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Error(messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Error);
		}

		[TestMethod()]
		public void ErrorExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Error(messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Error, ex);
		}

		[TestMethod()]
		public void ErrorExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Error(messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Error, ex, properties);
		}

		[TestMethod()]
		public void ErrorStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Error(category, messageData);
			AssertLogEntry(messageData, category, LogLevel.Error);
		}

		[TestMethod()]
		public void ErrorStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Error(category, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Error, ex);
		}

		[TestMethod()]
		public void ErrorStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Error(category, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Error, ex, properties);
		}

		[TestMethod()]
		public void FatalTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Fatal(messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Fatal);
		}

		[TestMethod()]
		public void FatalExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Fatal(messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Fatal, ex);
		}

		[TestMethod()]
		public void FatalExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Fatal(messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Fatal, ex, properties);
		}

		[TestMethod()]
		public void FatalStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Fatal(category, messageData);
			AssertLogEntry(messageData, category, LogLevel.Fatal);
		}

		[TestMethod()]
		public void FatalStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Fatal(category, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Fatal, ex);
		}

		[TestMethod()]
		public void FatalStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Fatal(category, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Fatal, ex, properties);
		}

		[TestMethod()]
		public void InfoTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Info(messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info);
		}

		[TestMethod()]
		public void InfoExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Info(messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info, ex);
		}

		[TestMethod()]
		public void InfoExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Info(messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info, ex, properties);
		}

		[TestMethod()]
		public void InfoStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Info(category, messageData);
			AssertLogEntry(messageData, category, LogLevel.Info);
		}

		[TestMethod()]
		public void InfoStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Info(category, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Info, ex);
		}

		[TestMethod()]
		public void InfoStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Info(category, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Info, ex, properties);
		}

		[TestMethod()]
		public void WarningTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Warning(messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Warning);
		}

		[TestMethod()]
		public void WarningExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Warning(messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Warning, ex);
		}

		[TestMethod()]
		public void WarningExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Warning(messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Warning, ex, properties);
		}

		[TestMethod()]
		public void WarningStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Warning(category, messageData);
			AssertLogEntry(messageData, category, LogLevel.Warning);
		}

		[TestMethod()]
		public void WarningStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Warning(category, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Warning, ex);
		}

		[TestMethod()]
		public void WarningStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Warning(category, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Warning, ex, properties);
		}

		[TestMethod()]
		public void WriteTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			target.Write(LogLevel.Info, messageData);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info);
		}

		[TestMethod()]
		public void WriteExTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			target.Write(LogLevel.Info, messageData, ex);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info, ex);
		}

		[TestMethod()]
		public void WriteExPropTest()
		{
			Log target = GetLog();
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			target.Write(LogLevel.Info, messageData, ex, properties);
			AssertLogEntry(messageData, target.CategoryName, LogLevel.Info, ex, properties);
		}

		[TestMethod()]
		public void WriteStaticTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Log.Write(category, LogLevel.Info, messageData);
			AssertLogEntry(messageData, category, LogLevel.Info);
		}

		[TestMethod()]
		public void WriteStaticExTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			Log.Write(category, LogLevel.Info, messageData, ex);
			AssertLogEntry(messageData, category, LogLevel.Info, ex);
		}

		[TestMethod()]
		public void WriteStaticExPropTest()
		{
			Type category = typeof(LogTest);
			object messageData = new TestData("Testing");
			Exception ex = new ArgumentNullException("arg");
			var properties = CreateTestProperties();
			Log.Write(category, LogLevel.Info, messageData, ex, properties);
			AssertLogEntry(messageData, category, LogLevel.Info, ex, properties);
		}

		[TestMethod()]
		public void GetLogStringTest()
		{
			string category = "Menees.Testing";
			Log actual = Log.GetLog(category);
			Assert.AreEqual(category, actual.CategoryName);
			// Pulling it again should return the same instance.
			Assert.AreEqual(actual, Log.GetLog(category));
		}

		[TestMethod()]
		public void GetLogTypeTest()
		{
			Type category = typeof(ApplicationInfo);
			Log actual = Log.GetLog(category);
			Assert.AreEqual(category.FullName, actual.CategoryName);
			// Pulling it again should return the same instance.
			Assert.AreEqual(actual, Log.GetLog(category));
		}

		[TestMethod()]
		public void CategoryNameTest()
		{
			Log target = Log.GetLog(typeof(Disposer));
			string actual = target.CategoryName;
			Assert.AreEqual(typeof(Disposer).FullName, actual);
		}

		[TestMethod()]
		public void IsDebugEnabledTest()
		{
			Log target = GetLog();
			bool actual = target.IsDebugEnabled;
			Assert.AreEqual(true, actual);
		}

		[TestMethod()]
		public void IsErrorEnabledTest()
		{
			Log target = GetLog();
			bool actual = target.IsErrorEnabled;
			Assert.AreEqual(true, actual);
		}

		[TestMethod()]
		public void IsFatalEnabledTest()
		{
			Log target = GetLog();
			bool actual= target.IsFatalEnabled;
			Assert.AreEqual(true, actual);
		}

		[TestMethod()]
		public void IsInfoEnabledTest()
		{
			Log target = GetLog();
			bool actual = target.IsInfoEnabled;
			Assert.AreEqual(true, actual);
		}

		[TestMethod()]
		public void IsWarningEnabledTest()
		{
			Log target = GetLog();
			bool actual = target.IsWarningEnabled;
			Assert.AreEqual(true, actual);
		}

		[TestMethod()]
		public void GlobalContextTest()
		{
			GlobalLogContext actual = Log.GlobalContext;
			Assert.IsNotNull(actual);
			// Pulling it again should return the same instance.
			Assert.AreEqual(actual, Log.GlobalContext);
		}

		[TestMethod()]
		public void ThreadContextTest()
		{
			ThreadLogContext actual = Log.ThreadContext;
			Assert.IsNotNull(actual);
			// Pulling it again should return the same instance.
			Assert.AreEqual(actual, Log.ThreadContext);
		}
	}
}
