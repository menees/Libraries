using Menees.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class GlobalLogContextTest
	{
		[TestMethod()]
		[DoNotParallelize]
		public void ClearTest()
		{
			GlobalLogContext target = Log.GlobalContext;
			string key = "ClearTest";
			target[key] = 1;
			target.Clear();
			Assert.IsFalse(target.ContainsKey(key));
		}

		[TestMethod()]
		public void ContainsKeyTest()
		{
			GlobalLogContext target = Log.GlobalContext;
			string key = "ContainsKeyTest";
			target[key] = 1;
			Assert.IsTrue(target.ContainsKey(key));
			target.Remove(key);
			Assert.IsFalse(target.ContainsKey(key));
		}

		[TestMethod()]
		public void RemoveTest()
		{
			GlobalLogContext target = Log.GlobalContext;
			string key = "RemoveTest";
			target[key] = 1;
			Assert.IsTrue(target.ContainsKey(key));
			target.Remove(key);
			Assert.IsFalse(target.ContainsKey(key));
		}

		[TestMethod()]
		public void TryGetValueTest()
		{
			GlobalLogContext target = Log.GlobalContext;
			string key = "TryGetValueTest";
			target[key] = 1;
			if (target.TryGetValue(key, out int value))
			{
				value.ShouldBe(1);
			}
			else
			{
				Assert.Fail("The key (" + key + ") should have been found.");
			}
			target.Remove(key);
			if (target.TryGetValue("Missing", out string? _))
			{
				Assert.Fail("The 'Missing' key should NOT be found.");
			}
		}

		[TestMethod()]
		public void ItemTest()
		{
			GlobalLogContext target = Log.GlobalContext;
			string key = "ItemTest";
			int expected = 123;
			target[key] = expected;
			int actual = (int)target[key];
			actual.ShouldBe(expected);
			target.Remove(key);
		}
	}
}
