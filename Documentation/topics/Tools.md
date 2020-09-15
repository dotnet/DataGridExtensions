### Additional Events
An extension method that provides some additional events that are useful when interacting with the columns of the data grid. 

The event provider watches for changes on any column and provides an event on the data grid level:

```c#
void Initialize(DataGrid dataGrid)
{
  var dataGridEvents= dataGrid.GetAdditionalEvents();
  dataGridEvents.ColumnVisibilityChanged += DataGrid_ColumnVisibilityChanged;
  dataGridEvents.ColumnActualWidthChanged += DataGrid_ColumnActualWidthChanged;
  dataGridEvents.ColumnDisplayIndexChanged += DataGrid_ColumnDisplayIndexChanged;
  dataGridEvents.ColumnSortDirectionChanged += DataGrid_ColumnSortDirectionChanged;
```

Now you will receive an event whenever the visibility, actual width or the display index of any column changes.

### Apply Initial Sorting
```xml
<DataGrid dgx:Tools.ApplyInitialSorting="True">
  <DataGrid.Columns>
    <DataGridTextColumn Header="Initially sorted"
                        SortDirection="Descending" />
 ```
This feature fixes the problem that if you have column definitions specifying a `SortDirection`, 
the columns will have the sorting mark in the column header, but the data is not sorted.

Specifying `dgx:Tools.ApplyInitialSorting="True"` at the data grid level will fix this.

### Force Commit On Lost Focus
```xml
<DataGridTextColumn dgx:Tools.ForceCommitOnLostFocus="True" />
```
If the data grid gets out of scope while a cell is still in edit mode, e.g. because you change the 
tab of the tab control containing data grid, the changes are lost.

Enabling `ForceCommitOnLostFocus` will trigger a commit of the cell/row, so the changes are preserved.


### Multi Line Editing

```xml
<DataGridTextColumn dgx:Tools.IsMultilineEditingEnabled="True" />
```
This will switch the text column to allow multi-line editing. 

Ctrl+Enter will create a new line.


