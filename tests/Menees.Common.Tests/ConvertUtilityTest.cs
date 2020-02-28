using System;
using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Menees.Common.Tests
{
	[TestClass()]
	public class ConvertUtilityTest
	{
		[TestMethod()]
		public void ToBooleanTest()
		{
			Assert.AreEqual(true, ConvertUtility.ToBoolean("Y"));
			Assert.AreEqual(true, ConvertUtility.ToBoolean("Yes"));
			Assert.AreEqual(true, ConvertUtility.ToBoolean("T"));
			Assert.AreEqual(true, ConvertUtility.ToBoolean("True"));
			Assert.AreEqual(true, ConvertUtility.ToBoolean("1"));

			Assert.AreEqual(false, ConvertUtility.ToBoolean("N"));
			Assert.AreEqual(false, ConvertUtility.ToBoolean("No"));
			Assert.AreEqual(false, ConvertUtility.ToBoolean("F"));
			Assert.AreEqual(false, ConvertUtility.ToBoolean("False"));
			Assert.AreEqual(false, ConvertUtility.ToBoolean("0"));
			Assert.AreEqual(false, ConvertUtility.ToBoolean("Testing"));

			Assert.AreEqual(true, ConvertUtility.ToBoolean("Testing", true));
		}

		[TestMethod()]
		public void ToInt32Test()
		{
			int actual = ConvertUtility.ToInt32("Invalid", 37);
			Assert.AreEqual(37, actual);

			actual = ConvertUtility.ToInt32("1234", 37);
			Assert.AreEqual(1234, actual);
		}

		[TestMethod]
		public void ConvertValueTest()
		{
			Point actual = (Point)ConvertUtility.ConvertValue("1, 2", typeof(Point));
			Point expected = new Point(1, 2);
			Assert.AreEqual(expected, actual);

			actual = ConvertUtility.ConvertValue<Point>("3, 4");
			expected = new Point(3, 4);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void IsNullTest()
		{
			int? nullInt = null;
			Assert.AreEqual(true, ConvertUtility.IsNull(nullInt));
			Assert.AreEqual(true, ConvertUtility.IsNull(null));
			Assert.AreEqual(true, ConvertUtility.IsNull(DBNull.Value));

			Assert.AreEqual(false, ConvertUtility.IsNull(0));
			Assert.AreEqual(false, ConvertUtility.IsNull("Test"));
		}
	}
}
