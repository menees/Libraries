using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using Shouldly;
using System.Diagnostics;
using System.Collections.Generic;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class ExceptionsTest
	{
		[TestMethod()]
		public void NewArgumentExceptionTest()
		{
			string message = "Testing";
			ArgumentException actual = Exceptions.NewArgumentException(message);
			Assert.AreEqual(message, actual.Message);

			actual = Exceptions.NewArgumentException(message, CreateProperties());
			actual.Message.ShouldBe(message);
		}

		[TestMethod()]
		public void NewArgumentNamedException()
		{
			string message = "Testing";
			string argName = "Param";
			ArgumentException actual = Exceptions.NewArgumentException(message, argName);
			RequireArgumentException(actual, message, argName);
			actual = Exceptions.NewArgumentException(message, argName, CreateProperties());
			RequireArgumentException(actual, message, argName);
		}

		[TestMethod()]
		public void NewArgumentNullExceptionTest()
		{
			ArgumentNullException actual = Exceptions.NewArgumentNullException("Param");
			RequireArgumentException(actual, "Value cannot be null.", "Param");

			actual = Exceptions.NewArgumentNullException("Param", CreateProperties());
			RequireArgumentException(actual, "Value cannot be null.", "Param");
		}

		internal static void RequireArgumentException(ArgumentException ex, string expectedBaseMessage, string expectedArgName)
		{
			// .NET Framework and .NET Core format the ArgumentException.Message differently.
			string frameworkMessage = $"{expectedBaseMessage} (Parameter '{expectedArgName}')";
			string coreMessage = $"{expectedBaseMessage}\r\nParameter name: {expectedArgName}";
			string actualMessage = ex.Message;
			(actualMessage == frameworkMessage || actualMessage == coreMessage).ShouldBeTrue(actualMessage);
		}

		[TestMethod()]
		public void LogTest()
		{
			string message = "Out of range";
			IndexOutOfRangeException ex = Exceptions.Log(new IndexOutOfRangeException(message));
			Assert.AreEqual(message, ex.Message);

			// The category parameter is only for logging.
			InvalidCastException castEx = Exceptions.Log(new InvalidCastException(message), typeof(ExceptionsTest));
			Assert.AreEqual(message, castEx.Message);

			ObjectDisposedException disposedEx = Exceptions.Log(new ObjectDisposedException("Testing"));
			disposedEx.ObjectName.ShouldBe("Testing");

			Exceptions.Log(ex, typeof(ExceptionsTest));

			Dictionary<string, object> properties = CreateProperties();
			Exceptions.Log(ex, properties);
			Exceptions.Log(ex, this.GetType(), properties);
		}

		[TestMethod()]
		public void NewInvalidOperationExceptionTest()
		{
			string message = "Invalid";
			InvalidOperationException actual = Exceptions.NewInvalidOperationException(message);
			Assert.AreEqual(message, actual.Message);

			actual = Exceptions.NewInvalidOperationException(message, CreateProperties());
			actual.Message.ShouldBe(message);
		}

		[TestMethod]
		public void ForEachTest()
		{
			Exception simple = new("Simple");
			StringBuilder sb = new();
			Exceptions.ForEach(simple, (ex, depth, parent) => sb.Append(' ', depth).Append(ex.Message).AppendLine());
			string output = sb.ToString();
			output.ShouldBe("Simple\r\n");
			Trace.Write(output);

			AggregateException aggregate = new("Root",
				new TaskCanceledException("FirstCancel"),
				new TargetInvocationException("Invocation", new InvalidOperationException("Invalid")),
				new TaskCanceledException("SecondCancel"));
			sb.Clear();
			Exceptions.ForEach(aggregate, (ex, depth) => sb.Append(' ', depth).Append(ex.Message).AppendLine());
			output = sb.ToString();
			CheckMessage(output, "Root\r\n FirstCancel\r\n Invocation\r\n  Invalid\r\n SecondCancel\r\n");
			Trace.Write(output);
		}

		[TestMethod]
		public void GetMessageTest()
		{
			Exception simple = new("Simple");
			string output = Exceptions.GetMessage(simple);
			output.ShouldBe("Simple");
			Trace.Write(output);

			AggregateException aggregate = new("Root",
				new TaskCanceledException("FirstCancel"),
				new TargetInvocationException("Invocation", new InvalidOperationException("Invalid")),
				new TaskCanceledException("SecondCancel"));
			output = Exceptions.GetMessage(aggregate);
			CheckMessage(output, "Root\r\n\tFirstCancel\r\n\tInvocation\r\n\t\tInvalid\r\n\tSecondCancel");
			Trace.Write(output);
		}

		private static void CheckMessage(string actualMessage, string expectedMessage)
		{
			// .NET Core's AggregateException.Message includes the inner exceptions in the first line of the message.
			string[] lines = actualMessage.Replace("\r\n", "\n").Split('\n');
			lines[0] = "Root";
			actualMessage = string.Join("\r\n", lines);

			actualMessage.ShouldBe(expectedMessage);
		}

		private static Dictionary<string, object> CreateProperties() => new() { { "A", 1 }, { "B", 2 } };
	}
}
