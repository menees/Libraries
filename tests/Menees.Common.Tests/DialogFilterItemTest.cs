using System;
using System.Linq;
using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class DialogFilterItemTest
	{
		[TestMethod]
		public void ConstructorTest()
		{
			DialogFilterItem item = new DialogFilterItem("cs");
			item.ItemName.ShouldContainIgnoringCase("C# Source Files");
			item.Masks.SequenceEqual(new[] { "*.cs" }).ShouldBeTrue();
			item.ToString().ShouldContainIgnoringCase("C# Source Files (*.cs)|*.cs");

			item = new DialogFilterItem("vb", false);
			item.ItemName.ShouldEqual("Visual Basic Source File");
			item.Masks.SequenceEqual(new[] { "*.vb" }).ShouldBeTrue();
			item.ToString().ShouldEqual("Visual Basic Source File (*.vb)|*.vb");

			item = new DialogFilterItem("Text Files", "txt");
			item.ItemName.ShouldEqual("Text Files");
			item.Masks.SequenceEqual(new[] { "*.txt" }).ShouldBeTrue();
			item.ToString().ShouldEqual("Text Files (*.txt)|*.txt");

			item = new DialogFilterItem("Testing Files", "txt", ".abc", "*.xyz");
			item.ItemName.ShouldEqual("Testing Files");
			item.Masks.SequenceEqual(new[] { "*.txt", "*.abc", "*.xyz" }).ShouldBeTrue();
			item.ToString().ShouldEqual("Testing Files (*.txt;*.abc;*.xyz)|*.txt;*.abc;*.xyz");
		}

		[TestMethod]
		public void JoinTest()
		{
			DialogFilterItem item = new DialogFilterItem("Text Files", "txt");
			string actual = DialogFilterItem.Join(item, DialogFilterItem.AllFiles);
			actual.ShouldEqual("Text Files (*.txt)|*.txt|All Files (*.*)|*.*");
		}
	}
}
