using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class ConditionsTest
	{
		[TestMethod()]
		public void RequireArgumentNamedTest()
		{
			Conditions.RequireArgument(true, "True", "None");

			try
			{
				Conditions.RequireArgument(false, "False", "None");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentException ex)
			{
				ExceptionsTest.RequireArgumentException(ex, "False", "None");
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks. This tests the exception we raised.
				Assert.AreEqual("None", ex.ParamName);
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
			}

			bool state = true;
			Conditions.RequireArgument(state, "True", nameof(state));

			try
			{
				state = false;
				Conditions.RequireArgument(state, "False", nameof(state));
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentException ex)
			{
				ExceptionsTest.RequireArgumentException(ex, "False", "state");
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks. This tests the exception we raised.
				Assert.AreEqual("state", ex.ParamName);
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
			}
		}

		[TestMethod()]
		public void RequireArgumentUnnamedTest()
		{
			Conditions.RequireArgument(true, "True");

			try
			{
				Conditions.RequireArgument(false, "False");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentException ex)
			{
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks. This tests the exception we raised.
				Assert.AreEqual("False", ex.Message);
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
				ex.ParamName.ShouldBeNullOrEmpty();
			}
		}

		[TestMethod()]
		public void RequireReferenceTest()
		{
			Conditions.RequireReference("Valid", "test");

			try
			{
				Conditions.RequireReference((string?)null, "test");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentNullException ex)
			{
				ex.ParamName.ShouldBe("test");
			}

			string? testRef = "Valid";
			Conditions.RequireReference(testRef, nameof(testRef));

			try
			{
				testRef = null;
				Conditions.RequireReference(testRef, nameof(testRef));
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentNullException ex)
			{
				ex.ParamName.ShouldBe("testRef");
			}
		}

		[TestMethod()]
		public void RequireStateTest()
		{
			Conditions.RequireState(true, "Valid");

			try
			{
				Conditions.RequireState(false, "Invalid");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (InvalidOperationException ex)
			{
#pragma warning disable MSTEST0058 // Do not use asserts in catch blocks. This tests the exception we raised.
				Assert.AreEqual("Invalid", ex.Message);
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
			}
		}

		[TestMethod()]
		public void RequireStringTest()
		{
			string test = "Valid";
			Conditions.RequireString(test, "test");

			try
			{
				Conditions.RequireString("", "test");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentException ex)
			{
				ex.ParamName.ShouldBe("test");
			}

			test = "Valid";
			Conditions.RequireString(test, nameof(test));

			try
			{
				test = string.Empty;
				Conditions.RequireString(test, nameof(test));
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentException ex)
			{
				ex.ParamName.ShouldBe("test");
			}
		}
	}
}
