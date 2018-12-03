using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Data;
using System.Linq;

namespace spartan.COBOL
{
	[TestClass]
	public class CobolTableTests
	{
		[TestMethod]
		public void CobolTable_AddRow_AddsCorrectNumberOfRows()
		{
			var table = new CobolTable<MyRow>();

			var row1 = new MyRow() { A = 10, B = 2 };
			table.AddRow(row1);

			var row2 = new MyRow() { A = 1000, B = 1000 };
			table.AddRow(row2);

			Assert.AreEqual(2, table.Rows.Count, "Incorrect table.Rows.Count");
		}

		[TestMethod]
		public void CobolTable_SumAllTableFields()
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
		public void CobolTable_WrapsExistingTable_WithSameColumnNamesAsPropertyNames()
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

		[TestMethod]
		public void CobolTable_CreatesTableWithPrimaryKey()
		{
			var persons = new Person[]
			{
				new Person() { ID = 1, Name = "Mike", Surname = "Bloomberg" },
				new Person() { ID = 2, Name = "Chose", Surname = "Pedro" }
			};

			CobolTable<Person> typedTable = CobolTable.Create(persons, "ID");

			Assert.AreEqual(2, typedTable.Rows.Count, "Incorrect number of rows");

			// Check column names
			DataTable table = typedTable.DataTable;
			Assert.AreEqual(3, table.Columns.Count, "Incorrect column count");
			Assert.AreEqual("ID", table.Columns[0].ColumnName, "Incorrect name for column 0");
			Assert.AreEqual("Name", table.Columns[1].ColumnName, "Incorrect name for column 1");
			Assert.AreEqual("Surname", table.Columns[2].ColumnName, "Incorrect name for column 2");

			// Check primary key
			Assert.AreEqual(1, table.PrimaryKey.Count(), "Incorrect PK columns count");
			Assert.AreEqual("ID", table.PrimaryKey[0].ColumnName, "Incorrect PK column name");

			// Check first row
			Person row0 = typedTable.Rows[0];
			Assert.AreEqual(1, row0.ID, "Incorrect rows[0].ID");
			Assert.AreEqual("Mike", row0.Name, "Incorrect rows[0].Name");
			Assert.AreEqual("Bloomberg", row0.Surname, "Incorrect rows[0].Surname");

			// Check second row
			Person row1 = typedTable.Rows[1];
			Assert.AreEqual(2, row1.ID, "Incorrect rows[1].ID");
			Assert.AreEqual("Chose", row1.Name, "Incorrect rows[1].Name");
			Assert.AreEqual("Pedro", row1.Surname, "Incorrect rows[1].Surname");
		}

		[TestMethod]
		public void CobolTable_WorksWithNullableTypes()
		{
			var persons = new RowWithNullables[]
			{
				new RowWithNullables() { N = 1, F = null },
				new RowWithNullables() { N = null, F = 1.0 }
			};

			CobolTable<RowWithNullables> typedTable = CobolTable.Create(persons, "ID");

			Assert.AreEqual(2, typedTable.Rows.Count, "Incorrect number of rows");

			// Check column names
			DataTable table = typedTable.DataTable;
			Assert.AreEqual(2, table.Columns.Count, "Incorrect column count");
			Assert.AreEqual("N", table.Columns[0].ColumnName, "Incorrect name for column 0");
			Assert.AreEqual("F", table.Columns[1].ColumnName, "Incorrect name for column 1");

			// Check first row
			RowWithNullables row0 = typedTable.Rows[0];
			Assert.AreEqual(1, row0.N, "Incorrect rows[0].N");
			Assert.AreEqual(null, row0.F, "Incorrect rows[0].F");

			// Check second row
			RowWithNullables row1 = typedTable.Rows[1];
			Assert.AreEqual(null, row1.N, "Incorrect rows[1].N");
			Assert.AreEqual(1.0, row1.F, "Incorrect rows[1].F");
		}

		[TestMethod]
		public void CobolTable_ConvertRow_Works()
		{
			var typedTable = new CobolTable<Person>();

			DataRow row = typedTable.DataTable.NewRow();
			row["ID"] = 123;
			row["Name"] = "Bob";
			row["Surname"] = null;

			Person person = typedTable.ConvertRow(row);

			Assert.IsNotNull(person);
			Assert.AreEqual(123, person.ID, "Incorrect person.ID");
			Assert.AreEqual("Bob", person.Name, "Incorrect person.Name");
			Assert.AreEqual(null, person.Surname, "Incorrect person.Surname");
		}

		[TestMethod]
		public void CobolTable_GetFieldNames()
		{
			string[] fieldNames = CobolTable.GetFieldNames<MyRow>();
			fieldNames.ShouldBeSubsetOf(new string[] { "A", "B" });
			(new string[] { "A", "B" }).ShouldBeSubsetOf(fieldNames);
		}

		#region Private classes

		/// <summary>Row class.</summary>
		private class MyRow
		{
			public int A { get; set; }
			public int B { get; set; }
		}

		/// <summary>Row class with nullable types.</summary>
		private class RowWithNullables
		{
			public int? N { get; set; }
			public double? F { get; set; }
		}

		private class Person
		{
			public int ID { get; set; }
			public string Name { get; set; }
			public string Surname { get; set; }
		}

		#endregion
	}
}