using System;
using System.Diagnostics;
using System.Management;
using Menees.Windows.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Windows.Tests
{
	[TestClass]
	public class WmiUtilityTest
	{
		[TestMethod]
		public void QueryForRecordsTest()
		{
			// This query should work and return one record.
			int currentProcessId = ApplicationInfo.ProcessId;
			bool foundRecord = false;
			WmiUtility.QueryForRecords(
				"SELECT ProcessId FROM Win32_Process where ProcessId = " + currentProcessId,
				record =>
				{
					record.GetInt32("ProcessId").ShouldEqual(currentProcessId);
					foundRecord = true;
				});
			foundRecord.ShouldBeTrue("Found first record");

			try
			{
				WmiUtility.QueryForRecords("Select DoesNotExist from Win32_DoesNotExist",
					record => Assert.Fail("No records should be found."));
				Assert.Fail("QueryForRecords should have thrown an exception.");
			}
			catch (ManagementException ex)
			{
				// This is expected since the "Win32_DoesNotExist" class doesn't exist.
				ex.Message.ShouldContain("Invalid class");
			}
		}

		[TestMethod]
		public void TryQueryForRecordsTest()
		{
			// This query should work and return one record.
			int currentProcessId = ApplicationInfo.ProcessId;
			bool foundRecord = false;
			bool result = WmiUtility.TryQueryForRecords(
				"SELECT ProcessId FROM Win32_Process where ProcessId = " + currentProcessId,
				record =>
				{
					record.GetInt32("ProcessId").ShouldEqual(currentProcessId);
					foundRecord = true;
				});
			result.ShouldBeTrue("First query");
			foundRecord.ShouldBeTrue("Found first record");

			result = WmiUtility.TryQueryForRecords("Select DoesNotExist from Win32_DoesNotExist",
				record => Assert.Fail("No records should be found."));
			result.ShouldBeFalse("Second query");
		}

		[TestMethod]
		public void TryActionTest()
		{
			bool result = WmiUtility.TryAction(() => WmiUtility.QueryForRecords("SELECT ProcessId FROM Win32_Process where ProcessId = 0", rec => { }));
			result.ShouldBeTrue("First query");

			result = WmiUtility.TryAction(() => WmiUtility.QueryForRecords("Select DoesNotExist from Win32_DoesNotExist", rec => { }));
			result.ShouldBeFalse("Second query");
		}
	}
}
