using Menees.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class ThreadLogContextTest
	{
		[TestMethod()]
		public void ClearTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "ClearTest";
			target[key] = 1;
			target.Clear();
			Assert.AreEqual(false, target.ContainsKey(key));
		}

		[TestMethod()]
		public void ContainsKeyTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "ContainsKeyTest";
			target[key] = 1;
			Assert.AreEqual(true, target.ContainsKey(key));
			target.Remove(key);
			Assert.AreEqual(false, target.ContainsKey(key));
		}

		[TestMethod()]
		public void RemoveTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "RemoveTest";
			target[key] = 1;
			Assert.AreEqual(true, target.ContainsKey(key));
			target.Remove(key);
			Assert.AreEqual(false, target.ContainsKey(key));
		}

		[TestMethod()]
		public void TryGetValueTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "TryGetValueTest";
			target[key] = 1;
			if (target.TryGetValue(key, out int value))
			{
				Assert.AreEqual(1, value);
			}
			else
			{
				Assert.Fail("The key (" + key + ") should have been found.");
			}
			target.Remove(key);
			if (target.TryGetValue("Missing", out string _))
			{
				Assert.Fail("The 'Missing' key should NOT be found.");
			}
		}

		[TestMethod()]
		public void ItemTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "ItemTest";
			int expected = 123;
			target[key] = expected;
			int actual = (int)target[key];
			Assert.AreEqual(expected, actual);
			target.Remove(key);
		}

		[TestMethod()]
		public void PushTest()
		{
			ThreadLogContext target = Log.ThreadContext;
			string key = "PushTest";
			object value = "PushedValue";
			using (target.Push(key, value))
			{
				object actual = target[key];
				Assert.AreEqual(value, actual);
			}
			Assert.IsFalse(target.ContainsKey(key));
		}
	}
}
