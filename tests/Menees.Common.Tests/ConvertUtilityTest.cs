using System;
using Menees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Shouldly;
using System.Linq;

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
			object? actualObject = ConvertUtility.ConvertValue("1, 2", typeof(Point));
			actualObject.ShouldNotBeNull();
			Point actual = (Point)actualObject!;
			Point expected = new(1, 2);
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

		[TestMethod]
		public void FromHexTest()
		{
			static void Check(byte[] actual, params byte[] expected)
			{
				CollectionAssert.AreEquivalent(actual, expected, "Expected: " + string.Join(" ", expected.Select(v => v.ToString("X2"))));
			}

			ConvertUtility.FromHex(null).ShouldBeNull("null input");
			Check(ConvertUtility.FromHex(string.Empty), CollectionUtility.EmptyArray<byte>());
			Check(ConvertUtility.FromHex("  \t  "), CollectionUtility.EmptyArray<byte>());
			Check(ConvertUtility.FromHex(" 1 "), 0x01);
			Check(ConvertUtility.FromHex("123"), 0x01, 0x23);
			Check(ConvertUtility.FromHex("0x123"), 0x01, 0x23);
			Check(ConvertUtility.FromHex("0x1234"), 0x12, 0x34);
			Check(ConvertUtility.FromHex("  0x 12 34  "), 0x12, 0x34);
			Check(ConvertUtility.FromHex("12:34:AB:cd"), 0x12, 0x34, 0xAB, 0xCD);
			Check(ConvertUtility.FromHex("8badbeef"), 0x8B, 0xAD, 0xBE, 0xEF);
			Check(ConvertUtility.FromHex("23BD3DAFB0CF81F4E70A30316B43840D"),
				0x23, 0xBD, 0x3D, 0xAF, 0xB0, 0xCF, 0x81, 0xF4, 0xE7, 0x0A, 0x30, 0x31, 0x6B, 0x43, 0x84, 0x0D);

			// Test some error cases and tell it not to throw.
			ConvertUtility.FromHex("Invalid", false).ShouldBeNull("Invalid input: junk");
			ConvertUtility.FromHex("0x123abcdefg", false).ShouldBeNull("Invalid input: g");

			// Test an error case that should throw.
			try
			{
				ConvertUtility.FromHex("0x38Special", true);
				Assert.Fail("This should not be reached");
			}
			catch(ArgumentException) { }
		}

		[TestMethod]
		public void ToHexTest()
		{
			ConvertUtility.ToHex(null).ShouldBeNull();
			ConvertUtility.ToHex(Enumerable.Empty<byte>()).ShouldBe(string.Empty);
			byte[] ateBadBeef = new byte[] { 0x8B, 0xAD, 0xBE, 0xEF};
			ConvertUtility.ToHex(ateBadBeef).ShouldBe("8BADBEEF");
			ConvertUtility.ToHex(ateBadBeef, ToHexOptions.None).ShouldBe("8BADBEEF");
			ConvertUtility.ToHex(ateBadBeef, ToHexOptions.Lowercase).ShouldBe("8badbeef");
			ConvertUtility.ToHex(ateBadBeef, ToHexOptions.Include0xPrefix).ShouldBe("0x8BADBEEF");
			ConvertUtility.ToHex(ateBadBeef, ToHexOptions.Include0xPrefix | ToHexOptions.Lowercase).ShouldBe("0x8badbeef");
		}

		[TestMethod]
		public void RoundToSecondsTest()
		{
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(1.0)).ShouldBe(TimeSpan.FromSeconds(1));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(1.4)).ShouldBe(TimeSpan.FromSeconds(1));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(1.5)).ShouldBe(TimeSpan.FromSeconds(2));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(1.6)).ShouldBe(TimeSpan.FromSeconds(2));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(-1.0)).ShouldBe(TimeSpan.FromSeconds(-1));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(-1.4)).ShouldBe(TimeSpan.FromSeconds(-1));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(-1.5)).ShouldBe(TimeSpan.FromSeconds(-2));
			ConvertUtility.RoundToSeconds(TimeSpan.FromSeconds(-1.6)).ShouldBe(TimeSpan.FromSeconds(-2));
		}

		[TestMethod]
		public void TruncateToSecondsTest()
		{
			ConvertUtility.TruncateToSeconds(TimeSpan.FromSeconds(1.0)).ShouldBe(TimeSpan.FromSeconds(1));
			ConvertUtility.TruncateToSeconds(TimeSpan.FromSeconds(1.5)).ShouldBe(TimeSpan.FromSeconds(1));
			ConvertUtility.TruncateToSeconds(TimeSpan.FromSeconds(-1.0)).ShouldBe(TimeSpan.FromSeconds(-1));
			ConvertUtility.TruncateToSeconds(TimeSpan.FromSeconds(-1.5)).ShouldBe(TimeSpan.FromSeconds(-1));
		}
	}
}
