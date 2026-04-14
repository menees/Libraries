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
				isDisposed.ShouldBe(target.IsDisposed);
			}
			Assert.IsTrue(isDisposed);
		}

		[TestMethod()]
		public void IsDisposedTest()
		{
			bool isDisposed = false;
			Disposer target = new(() => isDisposed = true);
			isDisposed.ShouldBe(target.IsDisposed);
			target.Dispose();
			isDisposed.ShouldBe(target.IsDisposed);
		}
	}
}
