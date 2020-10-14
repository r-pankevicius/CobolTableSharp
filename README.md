# CobolTableSharp
.NET [[DataTable](https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable?view=netcore-3.1)] wrapper exposing it as strongly typed rows collection.

Original source is [[here](https://gist.github.com/spartanthe/5154626)]. I did not invent it, I just adopted it.

## Boring initialization simplified
Instead of writing:
```csharp
var dt = new DataTable();
dt.Columns.Add("ID", typeof(int));
dt.Columns.Add("Name", typeof(string));
dt.Columns.Add("Surname", typeof(string));

var row = dt.NewRow();
row["ID"] = 1;
row["Name"] = "Mike";
row["Surname"] = "Lee";
dt.Rows.Add(row);

row = dt.NewRow();
row["ID"] = 2;
row["Name"] = "David";
row["Surname"] = "Rabinovich";
dt.Rows.Add(row);

row = dt.NewRow();
row["ID"] = 3;
row["Name"] = "Chuck";
row["Surname"] = "Norris";
dt.Rows.Add(row);
```
you write:
```csharp
class Person
{
	public int ID { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
}

var ct = new CobolTable<Person>();
ct.AddRows(
	new Person [] 
	{
		new Person { ID = 1, Name = "Mike", Surname = "Lee"},
		new Person { ID = 2, Name = "David", Surname = "Rabinovich"},
		new Person { ID = 3, Name = "Chuck", Surname = "Norris"},
	});
```
You also will get a compiler error trying to write `ID = "qwerty"` rather than runtime error at `row["ID"] = "qwerty"`;
