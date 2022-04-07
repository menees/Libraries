namespace Menees.Common.Tests
{
	#region Using Directives

	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Shouldly;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	[TestClass]
	public sealed class StringExtensionsTests
	{
		[TestMethod]
		public void IsWhiteSpaceTest()
		{
			((string?)null).IsWhiteSpace().ShouldBeTrue();
			string.Empty.IsWhiteSpace().ShouldBeTrue();
			" ".IsWhiteSpace().ShouldBeTrue();
			"X".IsWhiteSpace().ShouldBeFalse();
		}

		[TestMethod]
		public void IsNotWhiteSpaceTest()
		{
			((string?)null).IsNotWhiteSpace().ShouldBeFalse();
			string.Empty.IsNotWhiteSpace().ShouldBeFalse();
			" ".IsNotWhiteSpace().ShouldBeFalse();
			"X".IsNotWhiteSpace().ShouldBeTrue();
		}

		[TestMethod]
		public void IsEmptyTest()
		{
			((string?)null).IsEmpty().ShouldBeTrue();
			string.Empty.IsEmpty().ShouldBeTrue();
			" ".IsEmpty().ShouldBeFalse();
			"X".IsEmpty().ShouldBeFalse();
		}

		[TestMethod]
		public void IsNotEmptyTest()
		{
			((string?)null).IsNotEmpty().ShouldBeFalse();
			string.Empty.IsNotEmpty().ShouldBeFalse();
			" ".IsNotEmpty().ShouldBeTrue();
			"X".IsNotEmpty().ShouldBeTrue();
		}
	}
}
