using System;
using System.Collections.Generic;
using System.Management;
using Menees.Windows.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Menees.Windows.Tests
{
	[TestClass]
	public class WmiRecordTest
	{
		[TestMethod]
		public void GetValuesTest()
		{
			int processId = ApplicationInfo.ProcessId;
			bool foundRecord = false;
			bool result = WmiUtility.TryQueryForRecords(
				"select ProcessId, CreationDate, ExecutablePath, Handle, InstallDate from Win32_Process where ProcessId = " + processId,
				record =>
				{
					foundRecord = true;
					record.GetInt32("ProcessId").ShouldBe(processId);

					// The Handle is a string version of the process ID.
					record.GetInt64("Handle").ShouldBe(processId);
					record.GetInt64N("Handle").ShouldNotBeNull("Handle");
					record.GetString("Handle").ShouldBe(processId.ToString());

					DateTime creationDate = record.GetDateTime("CreationDate");
					creationDate.ShouldBeLessThan(DateTime.Now);

					DateTime? installDate = record.GetDateTimeN("InstallDate");
					installDate.ShouldBeNull("InstallDate");

					string? path = record.GetString("ExecutablePath");
					object? pathValue = record.GetValue("ExecutablePath");
					pathValue.ShouldBeOfType(typeof(string));
					path.ShouldBe(pathValue.ToString());

					// Internally, this calls GetValue, which tries to convert to the correct .NET type.
					object? coercedCreationDate = record["CreationDate"];
					coercedCreationDate.ShouldBeOfType(typeof(DateTime));
					coercedCreationDate.ShouldBe(creationDate);
				});
			foundRecord.ShouldBeTrue("Found first record");
			result.ShouldBeTrue("First query");

			foundRecord = false;
			result = WmiUtility.TryQueryForRecords("select EnableDaylightSavingsTime, DaylightInEffect from Win32_ComputerSystem", record =>
				{
					foundRecord = true;

					TimeZoneInfo local = TimeZoneInfo.Local;
					record.GetBoolean("EnableDaylightSavingsTime").ShouldBeTrue("EnableDaylightSavingsTime");
					if (local.Id == "Central Standard Time")
					{
						// DST should always be enabled in the Central time zone.  DST may not be in effect though.
						bool? daylightInEffect = record.GetBooleanN("DaylightInEffect");
						daylightInEffect.ShouldNotBeNull("DaylightInEffect");
						daylightInEffect.ShouldBe(local.IsDaylightSavingTime(DateTime.Now));
					}
					else if (local.Id == "UTC")
					{
						// When these tests run on GitHub servers, it's usually in UTC. UTC reports true for
						// EnableDaylightSavingsTime, but it returns null for DaylightInEffect. To see what it was
						// doing I added the following lines to my GitHub Actions YAML workflow:
						//		[TimeZoneInfo]::Local
						//		[TimeZoneInfo]::Local.IsDaylightSavingTime([DateTime]::Now)
						// Then I changed my time zone to UTC on my local machine to debug.
						record.GetBoolean("EnableDaylightSavingsTime").ShouldBeTrue("EnableDaylightSavingsTime");
						bool? daylightInEffect = record.GetBooleanN("DaylightInEffect");
						daylightInEffect.ShouldBeNull("DaylightInEffect");
					}
				});
			foundRecord.ShouldBeTrue("Found second record");
			result.ShouldBeTrue("Second query");

			// Note: I can't find any WMI value that returns an "Interval", so I can't test GetTimeSpan or GetTimeSpanN.
		}

		[TestMethod]
		public void InvokeMethodTest()
		{
			// Note: This query must include Handle even though it's just a string version of ProcessId.
			// The Handle is the "key" property for Win32_Process, so it is required in order to call the
			// GetOwner method on each ManagementObject below.
			// For query help see "WQL (SQL for WMI)"
			// http://msdn.microsoft.com/en-us/library/windows/desktop/aa394606.aspx
			bool foundRecord = false;
			bool result = WmiUtility.TryQueryForRecords(
				"SELECT Handle FROM Win32_Process where ProcessId = " + ApplicationInfo.ProcessId,
				record =>
				{
					foundRecord = true;

					// "GetOwner method of the Win32_Process class (Windows)"
					// http://msdn.microsoft.com/en-us/library/windows/desktop/aa390460(v=vs.85).aspx
					object[] args = new object[2];
					object gotOwner = record.InvokeMethod("GetOwner", args);
					Convert.ToInt32(gotOwner).ShouldBe(0);
					string.Equals((string)args[0], Environment.UserName, StringComparison.OrdinalIgnoreCase).ShouldBeTrue("User");
					string.Equals((string)args[1], Environment.UserDomainName, StringComparison.OrdinalIgnoreCase).ShouldBeTrue("Domain");

					IDictionary<string, object> output = record.InvokeMethod("GetOwner", (IDictionary<string, object>?)null);
					Convert.ToInt32(output["ReturnValue"]).ShouldBe(0);
					string.Equals((string)output["User"], Environment.UserName, StringComparison.OrdinalIgnoreCase).ShouldBeTrue("User");
					string.Equals((string)output["Domain"], Environment.UserDomainName, StringComparison.OrdinalIgnoreCase).ShouldBeTrue("Domain");
				});
			foundRecord.ShouldBeTrue("Found first record");
			result.ShouldBeTrue("First query");
		}

		[TestMethod]
		public void WrappedObjectTest()
		{
			bool foundRecord = false;
			bool result = WmiUtility.TryQueryForRecords(
				"SELECT ProcessId, CreationDate, ExecutablePath, Handle FROM Win32_Process where ProcessId = " + ApplicationInfo.ProcessId,
				record =>
				{
					foundRecord = true;
					record.WrappedObject.ShouldNotBeNull();
					record.WrappedObject.ShouldBeOfType(typeof(ManagementObject));
				});
			foundRecord.ShouldBeTrue("Found first record");
			result.ShouldBeTrue("First query");
		}
	}
}
