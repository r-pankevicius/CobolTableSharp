using Shouldly;
using System.Data;
using System.Linq;
using Xunit;

namespace spartan.COBOL
{
	public class CobolTableTests
	{
		[Fact]
		public void CobolTable_Ctor_WithIEnumerableParamWorks()
		{
			var table = new CobolTable<MyRow>(
				new MyRow[]
				{
					new MyRow() { A = 10, B = 2 },
					new MyRow() { A = 1000, B = 1000 }
				});

			table.Rows.Count.ShouldBe(2, "Incorrect table.Rows.Count");
		}

		[Fact]
		public void CobolTable_AddRow_AddsCorrectNumberOfRows()
		{
			var table = new CobolTable<MyRow>();

			var row1 = new MyRow() { A = 10, B = 2 };
			table.AddRow(row1);

			var row2 = new MyRow() { A = 1000, B = 1000 };
			table.AddRow(row2);

			table.Rows.Count.ShouldBe(2, "Incorrect table.Rows.Count");
		}

		[Fact]
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

			tableTotal.ShouldBe(2012, "Incorrect tableTotal");
		}

		[Fact]
		public void CobolTable_WrapsExistingTable_WithSameColumnNamesAsPropertyNames()
		{
			var dataTable = new DataTable();

			dataTable.Columns.Add("A", typeof(int));
			dataTable.Columns.Add("B", typeof(int));

			System.Data.DataRow newRow = dataTable.NewRow();
			newRow["A"] = 1;
			newRow["B"] = 2;
			dataTable.Rows.Add(newRow);

			var typedTable = new CobolTable<MyRow>(dataTable);

			typedTable.Rows.Count.ShouldBe(1, "Incorrect number of rows");

			// Check first row
			MyRow row = typedTable.Rows[0];
			row.A.ShouldBe(1, "Incorrect rows[0].A");
			row.B.ShouldBe(2, "Incorrect rows[0].B");

			// Assign new value vie DataRow
			dataTable.Rows[0]["A"] = 10;
			typedTable.Rows[0].A.ShouldBe(10, "Incorrect rows[0].A after assignment");
		}

		[Fact]
		public void CobolTable_CreatesTableWithPrimaryKey()
		{
			var persons = new Person[]
			{
				new Person { ID = 1, Name = "Mike", Surname = "Bloomberg" },
				new Person { ID = 2, Name = "Chose", Surname = "Pedro" }
			};

			var typedTable = CobolTable.Create(persons, "ID");

			typedTable.Rows.Count.ShouldBe(2, "Incorrect number of rows");

			// Check column names
			DataTable table = typedTable.DataTable;
			table.Columns.Count.ShouldBe(3, "Incorrect column count");
			table.Columns[0].ColumnName.ShouldBe("ID", "Incorrect name for column 0");
			table.Columns[1].ColumnName.ShouldBe("Name", "Incorrect name for column 1");
			table.Columns[2].ColumnName.ShouldBe("Surname", "Incorrect name for column 2");

			// Check primary key
			table.PrimaryKey.Count().ShouldBe(1, "Incorrect PK columns count");
			table.PrimaryKey[0].ColumnName.ShouldBe("ID", "Incorrect PK column name");

			// Check first row
			Person row0 = typedTable.Rows[0];
			row0.ID.ShouldBe(1, "Incorrect rows[0].ID");
			row0.Name.ShouldBe("Mike", "Incorrect rows[0].Name");
			row0.Surname.ShouldBe("Bloomberg", "Incorrect rows[0].Surname");

			// Check second row
			Person row1 = typedTable.Rows[1];
			row1.ID.ShouldBe(2, "Incorrect rows[1].ID");
			row1.Name.ShouldBe("Chose", "Incorrect rows[1].Name");
			row1.Surname.ShouldBe("Pedro", "Incorrect rows[1].Surname");
		}

		[Fact]
		public void CobolTable_WorksWithNullableTypes()
		{
			var persons = new RowWithNullables[]
			{
				new RowWithNullables { N = 1, F = null },
				new RowWithNullables { N = null, F = 1.0 }
			};

			var typedTable = CobolTable.Create(persons, "ID");

			typedTable.Rows.Count.ShouldBe(2, "Incorrect number of rows");

			// Check column names
			DataTable table = typedTable.DataTable;
			table.Columns.Count.ShouldBe(2, "Incorrect column count");
			table.Columns[0].ColumnName.ShouldBe("N", "Incorrect name for column 0");
			table.Columns[1].ColumnName.ShouldBe("F", "Incorrect name for column 1");

			// Check first row
			RowWithNullables row0 = typedTable.Rows[0];
			row0.N.ShouldBe(1, "Incorrect rows[0].N");
			row0.F.ShouldBeNull("Incorrect rows[0].F");

			// Check second row
			RowWithNullables row1 = typedTable.Rows[1];
			row1.N.ShouldBeNull("Incorrect rows[1].N");
			row1.F.ShouldBe(1.0, "Incorrect rows[1].F");
		}

		[Fact]
		public void CobolTable_ConvertRow_Works()
		{
			var typedTable = new CobolTable<Person>();

			DataRow row = typedTable.DataTable.NewRow();
			row["ID"] = 123;
			row["Name"] = "Bob";
			row["Surname"] = null;

			Person person = typedTable.ConvertRow(row);

			person.ShouldNotBeNull();
			person.ID.ShouldBe(123, "Incorrect person.ID");
			person.Name.ShouldBe("Bob", "Incorrect person.Name");
			person.Surname.ShouldBeNull("Incorrect person.Surname");
		}

		[Fact]
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
