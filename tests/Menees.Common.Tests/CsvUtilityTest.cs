﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Menees.Common.Tests
{
	[TestClass]
	public class CsvUtilityTest
	{
		[TestMethod]
		public void ReadLineStringTest()
		{
			var values = CsvUtility.ReadLine("A, B, C").ToArray();
			CollectionAssert.AreEqual(new[] { "A", "B", "C" }, values);

			values = CsvUtility.ReadLine("A, B, C", false).ToArray();
			CollectionAssert.AreEqual(new[] { "A", " B", " C" }, values);
		}

		[TestMethod]
		public void ReadLineReaderTest()
		{
			using (StringReader reader = new StringReader("A, B, C\r\nD, E, F\r\nG, \"H\r\nh\", \"I\ni\""))
			{
				var values = CsvUtility.ReadLine(reader).ToArray();
				CollectionAssert.AreEqual(new[] { "A", "B", "C" }, values);

				values = CsvUtility.ReadLine(reader).ToArray();
				CollectionAssert.AreEqual(new[] { "D", "E", "F" }, values);

				values = CsvUtility.ReadLine(reader).ToArray();
				CollectionAssert.AreEqual(new[] { "G", "H\r\nh", "I\r\ni" }, values);

				var nullValue = CsvUtility.ReadLine(reader);
				nullValue.ShouldBeNull();
			}
		}

		[TestMethod]
		public void WriteValueTest()
		{
			StringBuilder buffer = new StringBuilder();
			using (StringWriter writer = new StringWriter(buffer))
			{
				CsvUtility.WriteValue(writer, "A");
				buffer.ToString().ShouldEqual("A");
				buffer.Clear();

				// This should be quoted because it contains a comma.
				CsvUtility.WriteValue(writer, "A, B");
				buffer.ToString().ShouldEqual("\"A, B\"");
				buffer.Clear();

				// This contains a comma and double quotes.
				CsvUtility.WriteValue(writer, "He said, \"What?\"");
				buffer.ToString().ShouldEqual("\"He said, \"\"What?\"\"\"");
				buffer.Clear();
			}
		}

		[TestMethod]
		public void WriteFileTest()
		{
			string tableFileName = FileUtility.GetTempFileName(".csv");
			string readerFileName = FileUtility.GetTempFileName(".csv");
			try
			{
				DataTable table = CreateTestTable();
				CsvUtility.WriteTable(tableFileName, table);
				using (DataTableReader reader = new DataTableReader(table))
				{
					CsvUtility.WriteTable(readerFileName, reader);
				}

				TestWrites(File.ReadAllText(tableFileName), File.ReadAllText(readerFileName));
			}
			finally
			{
				FileUtility.DeleteFile(tableFileName);
				FileUtility.DeleteFile(readerFileName);
			}
		}

		[TestMethod]
		public void WriteWriterTest()
		{
			DataTable table = CreateTestTableAndText(out string tableContents);

			StringBuilder readerContents = new StringBuilder();
			using (StringWriter writer = new StringWriter(readerContents))
			using (DataTableReader reader = new DataTableReader(table))
			{
				CsvUtility.WriteTable(writer, reader);
			}

			TestWrites(tableContents, readerContents.ToString());
		}

		private static readonly object[,] TestData = 
		{
			{ "Id", "Name", "Cost" },
			{ 1, "Silly Putty", 4.99m },
			{ 2, "G.I. Joe, with \"Kung Fu\" grip", 19.99m },
			{ 3, "Atari 2600", 249.99m },
			{ 4, "Soccer ball\r\n(Futbol)", 14.99m }
		};

		private DataTable CreateTestTable()
		{
			DataTable result = new DataTable();
			var columns = result.Columns;
			int numColumns = TestData.GetLength(1);
			for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
			{
				// Get the name from row 0 and the type from row 1.
				columns.Add((string)TestData[0, columnIndex], TestData[1, columnIndex].GetType());
			}

			var rows = result.Rows;
			int numRows = TestData.GetLength(0);
			for (int rowIndex = 1; rowIndex < numRows; rowIndex++)
			{
				DataRow row = result.NewRow();
				for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
				{
					row[columnIndex] = TestData[rowIndex, columnIndex];
				}

				rows.Add(row);
			}

			return result;
		}

		private DataTable CreateTestTableAndText(out string tableText)
		{
			DataTable result = CreateTestTable();

			StringBuilder buffer = new StringBuilder();
			using (StringWriter writer = new StringWriter(buffer))
			{
				CsvUtility.WriteTable(writer, result);
			}

			tableText = buffer.ToString();
			return result;
		}

		private void TestWrites(string tableContents, string readerContents)
		{
			tableContents.ShouldEqual(readerContents);

			using (StringReader reader = new StringReader(readerContents))
			{
				IList<string> values;
				int rowIndex = 0;
				int numColumns = TestData.GetLength(1);
				while ((values = CsvUtility.ReadLine(reader)) != null)
				{
					values.Count.ShouldEqual(numColumns);
					for (int columnIndex = 0; columnIndex < numColumns; columnIndex++)
					{
						string value = values[columnIndex];
						string testValue = TestData[rowIndex, columnIndex].ToString();
						value.ShouldEqual(testValue);
					}

					rowIndex++;
				}

				rowIndex.ShouldEqual(TestData.GetLength(0));
			}
		}

		[TestMethod]
		public void WriteLineValuesTest()
		{
			StringBuilder buffer = new StringBuilder();
			using (StringWriter writer = new StringWriter(buffer))
			{
				CsvUtility.WriteLine(writer, new object[] { 1, "A", "B", "C", 4.2m });
				buffer.ToString().ShouldEqual("1,A,B,C,4.2\r\n");
				buffer.Clear();

				CsvUtility.WriteLine(writer, new object[] { 1, "A, \"B A\", C", 4.2m });
				buffer.ToString().ShouldEqual("1,\"A, \"\"B A\"\", C\",4.2\r\n");
				buffer.Clear();
			}
		}

		[TestMethod]
		public void ReadTableFileTest()
		{
			string tableFileName = FileUtility.GetTempFileName(".csv");
			try
			{
				DataTable table = CreateTestTable();
				CsvUtility.WriteTable(tableFileName, table);
				DataTable newTable = CsvUtility.ReadTable(tableFileName);
				TestReadTable(table, newTable);
			}
			finally
			{
				FileUtility.DeleteFile(tableFileName);
			}
		}

		private static void TestReadTable(DataTable oldTable, DataTable newTable, bool treatAsStrings = true)
		{
			int columnCount = oldTable.Columns.Count;
			columnCount.ShouldEqual(newTable.Columns.Count);

			int rowCount = oldTable.Rows.Count;
			rowCount.ShouldEqual(newTable.Rows.Count);

			for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
			{
				DataRow originalRow = oldTable.Rows[rowIndex];
				DataRow newRow = newTable.Rows[rowIndex];
				for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
				{
					object originalValue = originalRow[columnIndex];
					object newValue = newRow[columnIndex];

					if (treatAsStrings)
					{
						originalValue = originalValue.ToString();
						newValue = newValue.ToString();
					}

					originalValue.ShouldEqual(newValue);
				}
			}
		}

		[TestMethod]
		public void ReadTableReaderTest()
		{
			DataTable table = CreateTestTableAndText(out string tableText);

			using (StringReader reader = new StringReader(tableText))
			{
				DataTable newTable = CsvUtility.ReadTable(reader);
				TestReadTable(table, newTable);
			}
		}

		[TestMethod]
		public void ReadTableReaderCustomTest()
		{
			DataTable table = CreateTestTableAndText(out string tableText);

			using (StringReader reader = new StringReader(tableText))
			{
				DataTable newTable = CsvUtility.ReadTable(reader, preLoadTable =>
					{
						int columnCount = preLoadTable.Columns.Count;
						for (int i = 0; i < columnCount; i++)
						{
							// Get the column types from the first test data row (just like CreateTestTable()).
							preLoadTable.Columns[i].DataType = TestData[1, i].GetType();
						}

						preLoadTable.PrimaryKey = new[] { preLoadTable.Columns[0] };
					});

				TestReadTable(table, newTable, treatAsStrings: false);
			}
		}
	}
}
