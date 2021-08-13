namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    /// <summary>
    /// Extension methods for the data grid.
    /// </summary>
    public static class ExtensionMethods
    {
        #region Multi-line editing
        /// <summary>
        /// Tweaks the editing element style to enable multi-line editing.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <remarks>
        /// Ctrl+Return will add a new line when editing the cell.
        /// </remarks>
        public static void EnableMultilineEditing(this DataGridTextColumn column)
        {
            var textBoxStyle = new Style(typeof(TextBox), column.EditingElementStyle);
            var setters = textBoxStyle.Setters;

            setters.Add(new EventSetter(UIElement.PreviewKeyDownEvent, (KeyEventHandler)EditingElement_PreviewKeyDown));
            setters.Add(new Setter(TextBoxBase.AcceptsReturnProperty, true));

            textBoxStyle.Seal();

            column.EditingElementStyle = textBoxStyle;
        }

        private static void EditingElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            e.Handled = true;
            var editingElement = (TextBox)sender;

            if (IsKeyDown(Key.LeftCtrl) || IsKeyDown(Key.RightCtrl))
            {
                // Ctrl+Return adds a new line
                editingElement.SelectedText = Environment.NewLine;
                editingElement.SelectionLength = 0;
                editingElement.SelectionStart += Environment.NewLine.Length;
            }
            else
            {
                // Return without Ctrl: Forward to parent, grid should move focused cell down.
                if (editingElement.Parent is not FrameworkElement parent)
                    return;

                var args = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Return)
                {
                    RoutedEvent = UIElement.KeyDownEvent
                };

                parent.RaiseEvent(args);
            }
        }

        private static bool IsKeyDown(this Key key)
        {
            return (Keyboard.GetKeyStates(key) & KeyStates.Down) != 0;
        }

        #endregion

        #region Copy/Paste
        /// <summary>
        /// Determines whether the cell selection of the data grid is a rectangular range.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns><c>true</c> if the cell selection is a rectangular range.</returns>
        public static bool HasRectangularCellSelection(this DataGrid dataGrid)
        {
            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if (selectedCells == null || !selectedCells.Any())
                return false;

            var visibleColumnIndexes = dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => c.DisplayIndex)
                .OrderBy(displayIndex => displayIndex)
                .ToList();

            var rowIndexes = selectedCells
                .Select(cell => cell.Item)
                .Distinct()
                .Select(item => dataGrid.Items.IndexOf(item))
                .ToArray();

            var columnIndexes = selectedCells
                .Select(c => c.Column.DisplayIndex)
                .Distinct()
                .Select(i => visibleColumnIndexes.IndexOf(i))
                .ToArray();

            var rows = rowIndexes.Max() - rowIndexes.Min() + 1;
            var columns = columnIndexes.Max() - columnIndexes.Min() + 1;

            return selectedCells.Count == rows * columns;
        }

        /// <summary>
        /// Gets the content of the selected cells as a table of strings.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The selected cell content.</returns>
        public static IList<IList<string>>? GetCellSelection(this DataGrid dataGrid)
        {
            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
                return null;

            var orderedRows = selectedCells
                .GroupBy(i => i.Item)
                .OrderBy(i => dataGrid.Items.IndexOf(i.Key));

            return orderedRows.Select(GetRowContent).ToArray();
        }

        /// <summary>
        /// Replaces the selected cells with the data. Cell selection and the data table must have matching dimensions, either 1:n, n:1 or n:x*n.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="data">The data.</param>
        /// <remarks>The cell selection is assumed to be a rectangular area.</remarks>
        /// <returns><c>true</c> if the dimensions of data and cell selection did match and the cells data has been replaced; otherwise <c>false</c>.</returns>
        public static bool PasteCells(this DataGrid dataGrid, IList<IList<string>>? data)
        {
            if (data == null)
                return false;

            var numberOfDataRows = data.Count;
            if (data.Count < 1)
                return false;

            var firstRow = data[0];

            var numberOfDataColumns = firstRow.Count;

            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
                return false;

            var selectedColumns = selectedCells
                .Select(cellInfo => cellInfo.Column)
                .Distinct()
                .Where(column => column.Visibility == Visibility.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToArray();

            var selectedRows = selectedCells
                .Select(cellInfo => cellInfo.Item)
                .Distinct()
                .OrderBy(item => dataGrid.Items.IndexOf(item))
                .ToArray();

            if ((selectedColumns.Length == 1) && (selectedRows.Length == 1))
            {
                // n:1 => n:n, extend selection to match data
                var selectedColumn = selectedColumns[0];
                selectedColumns = dataGrid.Columns
                    .Where(col => col.DisplayIndex >= selectedColumn.DisplayIndex)
                    .OrderBy(col => col.DisplayIndex)
                    .Where(col => col.Visibility == Visibility.Visible)
                    .Take(numberOfDataColumns)
                    .ToArray();

                var selectedItem = selectedRows[0];
                selectedRows = dataGrid.Items
                    .Cast<object>()
                    .Skip(dataGrid.Items.IndexOf(selectedItem))
                    .Take(numberOfDataRows)
                    .ToArray();
            }

            var verticalFactor = selectedRows.Length / numberOfDataRows;
            if ((numberOfDataRows * verticalFactor) != selectedRows.Length)
                return false;

            var horizontalFactor = selectedColumns.Length / numberOfDataColumns;
            if ((numberOfDataColumns * horizontalFactor) != selectedColumns.Length)
                return false;

            // n:x*n
            // ReSharper disable InconsistentNaming
            foreach (var row in selectedRows.Zip(Repeat(data, verticalFactor), (Row, CellValues) => new { Row, CellValues }))
            {
                foreach (var column in selectedColumns.Zip(Repeat(row.CellValues, horizontalFactor), (DataGridColumn, CellValue) => new { DataGridColumn, CellValue }))
                {
                    column.DataGridColumn.OnPastingCellClipboardContent(row.Row, column.CellValue);
                }
            }
            // ReSharper restore InconsistentNaming

            return true;
        }


        /// <summary>
        /// Gets the selected cells that are in visible columns.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The selected cells of visible columns.</returns>
        public static IList<DataGridCellInfo>? GetVisibleSelectedCells(this DataGrid? dataGrid)
        {
            var selectedCells = dataGrid?.SelectedCells;

            return selectedCells?
                .Where(cellInfo => cellInfo.IsValid && cellInfo.Column is { Visibility: Visibility.Visible })
                .ToArray();
        }

        /// <summary>
        /// Determines whether it's safe to call "DataGrid.SelectAll()".
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>
        ///   <c>true</c> if it's safe to call "DataGrid.SelectAll(); otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSelectAll(this DataGrid dataGrid)
        {
            return dataGrid.SelectionMode != DataGridSelectionMode.Single && dataGrid.SelectionUnit != DataGridSelectionUnit.Cell;
        }


        private static IEnumerable<T> Repeat<T>(ICollection<T> source, int count) where T : class
        {
            for (var i = 0; i < count; i++)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        private static IList<string> GetRowContent(IGrouping<object, DataGridCellInfo> row)
        {
            return row
                .OrderBy(i => i.Column.DisplayIndex)
                .Select(i => i.Column.OnCopyingCellClipboardContent(i.Item))
                .Select(i => i?.ToString() ?? string.Empty)
                .ToArray();
        }

        #endregion
    }
}
