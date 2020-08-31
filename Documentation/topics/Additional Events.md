An extension method provides some additional events that are useful when interacting with the columns of the data grid. 
The event provider watches for changes on any column and provides an event on the data grid level:

```c#
void Initialize(DataGrid dataGrid)
{
  var dataGridEvents= dataGrid.GetAdditionalEvents();
  dataGridEvents.ColumnVisibilityChanged += DataGrid_ColumnVisibilityChanged;
  dataGridEvents.ColumnActualWidthChanged += DataGrid_ColumnActualWidthChanged;
  dataGridEvents.ColumnDisplayIndexChanged += DataGrid_ColumnDisplayIndexChanged;
```

Now you will receive an event whenever the visibility, actual width or the display index of any column changes.

