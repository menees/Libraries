using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class ReadOnlySetTest
	{
		private static ISet<string> CreateInstance()
		{
			ISet<string> value = new HashSet<string>() { "A", "B", "C" };
			return value.AsReadOnly();
		}

		[TestMethod()]
		public void ConstructorTest()
		{
			CreateInstance();
		}

		[TestMethod()]
		public void ContainsTest()
		{
			ISet<string> target = CreateInstance();
			Assert.IsTrue(target.Contains("B"));
			Assert.IsFalse(target.Contains("X"));
		}

		[TestMethod()]
		public void CopyToTest()
		{
			ISet<string> target = CreateInstance();
			string[] array = new string[target.Count];
			int arrayIndex = 0;
			target.CopyTo(array, arrayIndex);
			Assert.IsTrue(target.SetEquals(array));
		}

		[TestMethod()]
		public void GetEnumeratorTest()
		{
			ISet<string> target = CreateInstance();
			foreach (var item in target)
			{
				Assert.IsNotNull(item);
			}
		}

		[TestMethod()]
		public void IsProperSubsetOfTest()
		{
			ISet<string> target = CreateInstance();
			bool actual = target.IsProperSubsetOf(new[] { "A", "B", "C", "D" });
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void IsProperSupersetOfTest()
		{
			ISet<string> target = CreateInstance();
			bool actual = target.IsProperSupersetOf(new[] { "A", "B" });
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void IsSubsetOfTest()
		{
			ISet<string> target = CreateInstance();
			bool actual = target.IsSubsetOf(new[] { "A", "B", "C" });
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void IsSupersetOfTest()
		{
			ISet<string> target = CreateInstance();
			bool actual = target.IsSupersetOf(new[] { "A", "B", "C" });
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void OverlapsTest()
		{
			ISet<string> target = CreateInstance();
			bool actual = target.Overlaps(new[] { "B", "C", "D" });
			Assert.IsTrue(actual);
		}

		[TestMethod()]
		public void SetEqualsTest()
		{
			ISet<string> target = CreateInstance();

			string[] other = new[] { "B", "A", "C" };
			bool actual = target.SetEquals(other);
			Assert.IsTrue(actual);

			other[2] = "D";
			actual = target.SetEquals(other);
			Assert.IsFalse(actual);
		}

		[TestMethod()]
		public void CountTest()
		{
			ISet<string> target = CreateInstance();
			int actual = target.Count;
			Assert.AreEqual(3, actual);
		}

		[TestMethod()]
		public void IsReadOnlyTest()
		{
			ISet<string> target = CreateInstance();
			Assert.IsTrue(target.IsReadOnly);
		}
	}
}
