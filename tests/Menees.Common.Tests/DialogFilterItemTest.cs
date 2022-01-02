using System;
using System.Linq;
using Menees.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Common.Tests
{
	[TestClass]
	public class DialogFilterItemTest
	{
		[TestMethod]
		public void ConstructorTest()
		{
			DialogFilterItem item = new("cs");
			item.ItemName.ShouldContain("C# Source Files", Case.Insensitive);
			item.Masks.SequenceEqual(new[] { "*.cs" }).ShouldBeTrue();
			item.ToString().ShouldContain("C# Source Files (*.cs)|*.cs", Case.Insensitive);

			item = new DialogFilterItem("vb", false);
			item.ItemName.ShouldBe("Visual Basic Source File");
			item.Masks.SequenceEqual(new[] { "*.vb" }).ShouldBeTrue();
			item.ToString().ShouldBe("Visual Basic Source File (*.vb)|*.vb");

			item = new DialogFilterItem("Text Files", "txt");
			item.ItemName.ShouldBe("Text Files");
			item.Masks.SequenceEqual(new[] { "*.txt" }).ShouldBeTrue();
			item.ToString().ShouldBe("Text Files (*.txt)|*.txt");

			item = new DialogFilterItem("Testing Files", "txt", ".abc", "*.xyz");
			item.ItemName.ShouldBe("Testing Files");
			item.Masks.SequenceEqual(new[] { "*.txt", "*.abc", "*.xyz" }).ShouldBeTrue();
			item.ToString().ShouldBe("Testing Files (*.txt;*.abc;*.xyz)|*.txt;*.abc;*.xyz");
		}

		[TestMethod]
		public void JoinTest()
		{
			DialogFilterItem item = new("Text Files", "txt");
			string actual = DialogFilterItem.Join(item, DialogFilterItem.AllFiles);
			actual.ShouldBe("Text Files (*.txt)|*.txt|All Files (*.*)|*.*");
		}
	}
}
