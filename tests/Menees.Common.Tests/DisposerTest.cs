using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class DisposerTest
	{
		[TestMethod()]
		public void DisposeTest()
		{
			bool isDisposed = false;
			using (Disposer target = new(() => isDisposed = true))
			{
				Assert.AreEqual(target.IsDisposed, isDisposed);
			}
			Assert.AreEqual(true, isDisposed);
		}

		[TestMethod()]
		public void IsDisposedTest()
		{
			bool isDisposed = false;
			Disposer target = new(() => isDisposed = true);
			Assert.AreEqual(target.IsDisposed, isDisposed);
			target.Dispose();
			Assert.AreEqual(target.IsDisposed, isDisposed);
		}
	}
}
