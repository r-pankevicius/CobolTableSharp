using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace spartan.COBOL
{
	[TestClass]
	public class CobolTableTests
	{
		/// <summary>Row class.</summary>
		private class MyRow
		{
			public int A { get; set; }
			public int B { get; set; }
		}


		[TestMethod]
		public void AddRow_AddsCorrectNumberOfRows()
		{
			var table = new CobolTable<MyRow>();

			var row1 = new MyRow() { A = 10, B = 2 };
			table.AddRow(row1);

			var row2 = new MyRow() { A = 1000, B = 1000 };
			table.AddRow(row2);

			Assert.AreEqual(2, table.Rows.Count, "Incorrect table.Rows.Count");
		}

		[TestMethod]
		public void TestSumAllTableFields()
		{
			var table = new CobolTable<MyRow>();

			var row1 = new MyRow() { A = 10, B = 2 };
			table.AddRow(row1);

			var row2 = new MyRow() { A = 1000, B = 1000 };
			table.AddRow(row2);

			int tableTotal = 0;

			foreach (MyRow row in table.Rows)
				tableTotal += row.A + row.B;

			Assert.AreEqual(2012, tableTotal, "Incorrect tableTotal");
		}

		[TestMethod]
		public void WrapsExistingTable_WithSameColumnNamesAsPropertyNames()
		{
			var dataTable = new System.Data.DataTable();

			dataTable.Columns.Add("A", typeof(int));
			dataTable.Columns.Add("B", typeof(int));

			System.Data.DataRow newRow = dataTable.NewRow();
			newRow["A"] = 1;
			newRow["B"] = 2;
			dataTable.Rows.Add(newRow);

			var typedTable = new CobolTable<MyRow>(dataTable);

			Assert.AreEqual(1, typedTable.Rows.Count, "Incorrect number of rows");

			// Check first row
			MyRow row = typedTable.Rows[0];
			Assert.AreEqual(1, row.A, "Incorrect rows[0].A");
			Assert.AreEqual(2, row.B, "Incorrect rows[0].B");

			// Assign new value vie DataRow
			dataTable.Rows[0]["A"] = 10;
			Assert.AreEqual(10, typedTable.Rows[0].A, "Incorrect rows[0].A after assignment");
		}
	}
}