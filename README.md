# CobolTableSharp
.NET DataTable wrapper exposing it as strongly typed rows collection.

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
```
you write:
```csharp
class Person
{
	public int ID { get; set; }
	public string Name { get; set; }
	public string Surname { get; set; }
}
// ...
var ct = new CobolTable<Person>();
ct.AddRow(new Person
{
	ID = 1,
	Name = "Mike",
	Surname = "Lee"
});
```