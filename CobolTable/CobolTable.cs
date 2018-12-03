using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Data;

namespace spartan.COBOL
{
	/// <summary>
	/// DataTable wrapper exposing strongly typed rows collection.
	/// TRow property names are same as data table's column names.
	/// </summary>
	/// <typeparam name="TRow">Table row type</typeparam>
	/// <remarks>
	/// Intended use case: when it's difficult to find uses of dynamic tables
	/// passed around everywhere, it should help you to sort things out,
	/// step by step.
	/// </remarks>
	public class CobolTable<TRow> where TRow : new()
	{
		private DataTable table;

		/// <summary>
		/// Constructs new empty typed table.
		/// </summary>
		public CobolTable()
		{
			table = new DataTable();

			foreach (Column col in TableStructure<TRow>.Columns)
			{
				table.Columns.Add(new DataColumn(col.Name, col.Type));
			}
		}

		/// <summary>
		/// Constructs a typed table over an existing data table <paramref name="dataTable"/>.
		/// </summary>
		/// <param name="dataTable">Data table</param>
		/// <remarks>
		/// Data table structure must match TRow (to the extent triggered by code).
		/// Ideally TRow property names are same as data table's column names.
		/// </remarks>
		public CobolTable(DataTable dataTable)
			: base()
		{
			if (dataTable == null)
				throw new ArgumentNullException("dataTable");

			this.table = dataTable;
		}

		/// <summary>
		/// Returns wrapped data table.
		/// </summary>
		/// <remarks>
		/// Use to pass it to older method accepting dynamic data table in middle of refactoring.
		/// </remarks>
		public DataTable DataTable
		{
			get { return this.table; }
		}

		public TypedRowCollection Rows
		{
			get { return new TypedRowCollection(table.Rows); }
		}

		public void AddRow(TRow newRow)
		{
			DataRow tableRow = table.NewRow();
			CopyToDataRow(tableRow, newRow);
			table.Rows.Add(tableRow);
		}

		#region Implementation

		private static TRow Extract(DataRow tableRow)
		{
			TRow row = new TRow();
			CopyFromDataRow(row, tableRow);
			return row;
		}

		/// <summary>
		/// Copies DataRow row <paramref name="src"/>
		/// to typed row <paramref name="dest"/>.
		/// </summary>
		private static void CopyFromDataRow(TRow dest, DataRow src)
		{
			foreach (Column col in TableStructure<TRow>.Columns)
				col.CopyFromDataRow(dest, src);
		}

		/// <summary>
		/// Copies typed row <paramref name="src"/>
		/// to DataTable row <paramref name="dest"/>.
		/// </summary>
		private static void CopyToDataRow(DataRow dest, TRow src)
		{
			foreach (Column col in TableStructure<TRow>.Columns)
				col.CopyToDataRow(dest, src);
		}

		#endregion

		#region struct TypedRowCollection

		/// <summary>
		/// Typed rows wrapper over <see cref="System.Data.DataRowCollection"/>.
		/// </summary>
		public struct TypedRowCollection
		{
			DataRowCollection rows;

			public TypedRowCollection(DataRowCollection rows)
			{
				this.rows = rows;
			}

			public int Count { get { return this.rows.Count; } }

			public TRow this[int index]
			{
				get
				{
					DataRow row = this.rows[index];
					TRow typedRow = Extract(row);
					return typedRow;
				}
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(rows.GetEnumerator());
			}

			/// <summary>
			/// Enumerator (foreach support)
			/// </summary>
			public class Enumerator
			{
				IEnumerator rowsEnumerator;

				public Enumerator(IEnumerator rowsEnumerator)
				{
					this.rowsEnumerator = rowsEnumerator;
				}

				public TRow Current
				{
					get
					{
						return Extract((DataRow)rowsEnumerator.Current);
					}
				}

				public bool MoveNext()
				{
					return rowsEnumerator.MoveNext();
				}
			}
		}

		#endregion struct TypedRowCollection

		#region class TableStructure

		/// <summary>
		/// Shared type-2-table mapping structure.
		/// </summary>
		/// <typeparam name="TRow2">Type of "typed row".</typeparam>
		private class TableStructure<TRow2> where TRow2 : new()
		{
			static List<Column> sColumns;

			// TODO: R/O coll
			public static List<Column> Columns
			{
				get
				{
					if (sColumns == null)
						sColumns = Column.GetRowColumns();

					return sColumns;
				}
			}
		}

		#endregion class TableStructure

		#region class Column

		/// <summary>
		/// Describes public properties of typed row, provides attribute copiers between 
		/// data table columns and TRow instances.
		/// </summary>
		private class Column
		{
			private static Dictionary<Type, List<Column>> TypeToColumnsCache =
				new Dictionary<Type, List<Column>>();

			/// <summary>
			/// Attribute getter. Reads attribute from the instance.
			/// value = getter(row) ::= value = row.attr.
			/// </summary>
			private MethodInfo attributeGetter;

			/// <summary>
			/// Attribute setter. Assigns property to the instance.
			/// setter(row, value) ::= row.attr = value.
			/// </summary>
			private MethodInfo attributeSetter;

			/// <summary>
			/// Attribute name; same as table column name.
			/// </summary>
			public string Name { get; private set; }

			/// <summary>
			/// Attribute type; same as table column data type.
			/// </summary>
			public Type Type { get; private set; }

			/// <summary>
			/// Copies properties from typed instance to data row.
			/// </summary>
			/// <param name="dest">To: Table data row</param>
			/// <param name="src">From: typed instance</param>
			internal void CopyToDataRow(DataRow dest, TRow src)
			{
				object attributeValue = attributeGetter.Invoke(src, new object[] { });
				object dbValue = (attributeValue ?? DBNull.Value);
				dest[Name] = dbValue;
			}

			/// <summary>
			/// Copies properties from data row to typed instance.
			/// </summary>
			/// <param name="src">From: Table data row</param>
			/// <param name="dest">To: typed instance</param>
			internal void CopyFromDataRow(TRow dest, DataRow src)
			{
				object dbValue = src[Name];
				object attributeValue = (dbValue != DBNull.Value) ? dbValue : null;
				attributeSetter.Invoke(dest, new object[] { attributeValue });
			}

			// = Static, Cache

			/// <summary>
			/// Parses TRow public properties to find out what "data columns"
			/// are needed for typed table.
			/// </summary>
			/// <returns>Data columns needed for typed table.</returns>
			internal static List<Column> GetRowColumns()
			{
				return Parse(typeof(TRow));
			}

			#region Implementation

			/// <summary>
			/// Reflect public get/set properties of <paramref name="rowType"/>
			/// as column descriptors.
			/// Tries type cache first.
			/// </summary>
			/// <param name="rowType">'Row' type</param>
			/// <returns>A list if column descriptors</returns>
			private static List<Column> Parse(Type rowType)
			{
				List<Column> columns = null;

				if (!TypeToColumnsCache.TryGetValue(rowType, out columns))
				{
					columns = ParseType(rowType);
					TypeToColumnsCache[rowType] = columns;
				}

				return columns;
			}

			/// <summary>
			/// Same as Parse, but doesn't use type cache.
			/// </summary>
			private static List<Column> ParseType(Type rowType)
			{
				var result = new List<Column>();

				// Public R/W properties
				foreach (PropertyInfo pi in rowType.GetProperties())
				{
					if (pi.CanRead && pi.CanWrite)
					{
						Column col = new Column();

						col.Name = pi.Name;
						col.attributeGetter = pi.GetGetMethod();
						col.attributeSetter = pi.GetSetMethod();
						col.Type = pi.PropertyType;

						result.Add(col);
					}
				}

				return result;
			}

			#endregion
		}

		#endregion class Column
	}
}