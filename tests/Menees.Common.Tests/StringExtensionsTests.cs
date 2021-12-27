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
		public void IsBlankTest()
		{
			((string?)null).IsBlank().ShouldBeTrue();
			string.Empty.IsBlank().ShouldBeTrue();
			" ".IsBlank().ShouldBeTrue();
			"X".IsBlank().ShouldBeFalse();
		}

		[TestMethod]
		public void IsNotBlankTest()
		{
			((string?)null).IsNotBlank().ShouldBeFalse();
			string.Empty.IsNotBlank().ShouldBeFalse();
			" ".IsNotBlank().ShouldBeFalse();
			"X".IsNotBlank().ShouldBeTrue();
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
