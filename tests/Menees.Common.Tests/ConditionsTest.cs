using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SoftwareApproach.TestingExtensions;

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
				Assert.AreEqual("None", ex.ParamName);
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
				Assert.AreEqual("state", ex.ParamName);
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
				Assert.AreEqual("False", ex.Message);
				Assert.IsNull(ex.ParamName);
			}
		}

		[TestMethod()]
		public void RequireReferenceTest()
		{
			Conditions.RequireReference("Valid", "test");

			try
			{
				Conditions.RequireReference((string)null, "test");
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentNullException ex)
			{
				ex.ParamName.ShouldEqual("test");
			}

			string testRef = "Valid";
			Conditions.RequireReference(testRef, nameof(testRef));

			try
			{
				testRef = null;
				Conditions.RequireReference(testRef, nameof(testRef));
				Assert.Fail("An exception should have been thrown before this.");
			}
			catch (ArgumentNullException ex)
			{
				ex.ParamName.ShouldEqual("testRef");
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
				Assert.AreEqual("Invalid", ex.Message);
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
				ex.ParamName.ShouldEqual("test");
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
				ex.ParamName.ShouldEqual("test");
			}
		}
	}
}
