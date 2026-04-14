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
				ex.ParamName.ShouldBe("None");
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
				ex.ParamName.ShouldBe("state");
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
				ex.Message.ShouldBe("False");
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
				ex.ParamName.ShouldBeNullOrEmpty();
			}
		}

		[TestMethod]
		public void RequireCollectionTest()
		{
			string[] array = ["X "];
			Conditions.RequireCollection(array, "test").ShouldBe(array);

			Should.Throw<ArgumentException>(() => Conditions.RequireCollection<int>(null, "test"))
				.ParamName.ShouldBe("test");

			Should.Throw<ArgumentException>(() => Conditions.RequireCollection(Array.Empty<int>(), "test"))
				.ParamName.ShouldBe("test");
		}


	[TestMethod]
	public void RequireReferenceTest()
	{
		Conditions.RequireReference("Valid", "test");

		Should.Throw<ArgumentNullException>(() => Conditions.RequireReference((string?)null, "test")).ParamName.ShouldBe("test");

		string? testRef = "Valid";
		Conditions.RequireReference(testRef, nameof(testRef)).ShouldBe(testRef);

		testRef = null;
		Should.Throw<ArgumentNullException>(() => Conditions.RequireReference(testRef, nameof(testRef))).ParamName.ShouldBe("testRef");

		int? testValue = 123;
		Conditions.RequireReference(testValue, nameof(testValue)).ShouldBe(123);

		// Depends on C# 10's [CallerArgumentExpression] support.
		Should.Throw<ArgumentNullException>(() => Conditions.RequireReference(testRef)).ParamName.ShouldBe("testRef");
	}


		[TestMethod]
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
				ex.Message.ShouldBe("Invalid");
#pragma warning restore MSTEST0058 // Do not use asserts in catch blocks
			}
		}

		[TestMethod]
		public void RequireStringTest()
		{
			string test = "Valid";
			Conditions.RequireString(test, nameof(test)).ShouldBe(test);

			Should.Throw<ArgumentException>(() => Conditions.RequireString(null, "test")).ParamName.ShouldBe("test");
			Should.Throw<ArgumentException>(() => Conditions.RequireString(string.Empty, "test")).ParamName.ShouldBe("test");

			test = string.Empty;
			Should.Throw<ArgumentException>(() => Conditions.RequireString(test, nameof(test))).ParamName.ShouldBe("test");

			// Depends on C# 10's [CallerArgumentExpression] support.
			Should.Throw<ArgumentException>(() => Conditions.RequireString(test)).ParamName.ShouldBe("test");
		}

	}
}
