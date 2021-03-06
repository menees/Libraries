﻿using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class CollectionUtilityTest
	{
		[TestMethod()]
		public void AsReadOnlySetTest()
		{
			ISet<string> value = new HashSet<string>() { "A", "B", "C" };
			Assert.IsFalse(value.IsReadOnly);
			ISet<string> actual = value.AsReadOnly();
			Assert.IsTrue(actual.IsReadOnly);
		}

		[TestMethod()]
		public void AsReadOnlyDictionaryTest()
		{
			IDictionary<string, string> dictionary = new Dictionary<string, string>() { { "A", "a" }, { "B", "b" }, { "C", "c" } };
			Assert.IsFalse(dictionary.IsReadOnly);
			IDictionary<string, string> actual = dictionary.AsReadOnly();
			Assert.IsTrue(actual.IsReadOnly);
		}

		[TestMethod]
		public void EmptyArrayTest()
		{
			int[] ints = CollectionUtility.EmptyArray<int>();
			ints.Length.ShouldEqual(0);

			string[] strings = CollectionUtility.EmptyArray<string>();
			strings.Length.ShouldEqual(0);

			string[] secondStrings = CollectionUtility.EmptyArray<string>();
			object.ReferenceEquals(strings, secondStrings).ShouldBeTrue("The cache should return the same instance.");

			Array.Resize(ref strings, 1);
			object.ReferenceEquals(strings, CollectionUtility.EmptyArray<string>()).ShouldBeFalse("Resize shouldn't affect the cache.");
			object.ReferenceEquals(secondStrings, CollectionUtility.EmptyArray<string>()).ShouldBeTrue("The cache should still be the same.");
		}
	}
}
