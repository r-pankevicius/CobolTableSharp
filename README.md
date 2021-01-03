# CobolTableSharp
.NET [[DataTable](https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable?view=netcore-3.1)] wrapper exposing it as strongly typed rows collection.

Original source is [[here](https://gist.github.com/spartanthe/5154626)]. I did not invent it, I just adopted it.

## Boring initialization simplified
Instead of writing:
```csharp
var dataTable = new DataTable();
dataTable.Columns.Add("ID", typeof(int));
dataTable.Columns.Add("Name", typeof(string));
dataTable.Columns.Add("Surname", typeof(string));

var row = dataTable.NewRow();
row["ID"] = 1;
row["Name"] = "Mike";
row["Surname"] = "Lee";
dataTable.Rows.Add(row);

row = dataTable.NewRow();
row["ID"] = 2;
row["Name"] = "David";
row["Surname"] = "Rabinovich";
dataTable.Rows.Add(row);

row = dataTable.NewRow();
row["ID"] = 3;
row["Name"] = "Chuck";
row["Surname"] = "Norris";
dataTable.Rows.Add(row);
```
you write:
```csharp
class Person
{
	public int ID { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
}

var cobolTable = new CobolTable<Person>(
	new Person [] 
	{
		new Person { ID = 1, Name = "Mike", Surname = "Lee"},
		new Person { ID = 2, Name = "David", Surname = "Rabinovich"},
		new Person { ID = 3, Name = "Chuck", Surname = "Norris"},
	});

// And your DataTable is here:
var dt = cobolTable.DataTable;
```
You also will get a compiler error trying to write `ID = "qwerty"` rather than runtime error at `row["ID"] = "qwerty"`;

## DataTable last, not first in reports

Many report engines prefer DataTable as input. If you start from "DataTable first" design then after few years people will not understand what's going on here. Especially if you fetch some data from database, some data from services, append some data... CobolTable let's you focus on types, use Linq, and aggregate all stuff to your reports engine as a last step. Everything is typed before the last step.

## TBD: other goodies

Take a look at [[tests](CobolTable.Tests/CobolTableTests.cs)] to understand how it can be used.
