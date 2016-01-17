namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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
        // ReSharper disable once SuggestBaseTypeForParameter : works only with text column!
        public static void EnableMultilineEditing(this DataGridTextColumn column)
        {
            Contract.Requires(column != null);

            var textBoxStyle = new Style(typeof(TextBox), column.EditingElementStyle);
            var setters = textBoxStyle.Setters;

            setters.Add(new EventSetter(UIElement.PreviewKeyDownEvent, (KeyEventHandler)EditingElement_PreviewKeyDown));
            setters.Add(new Setter(TextBoxBase.AcceptsReturnProperty, true));

            textBoxStyle.Seal();

            column.EditingElementStyle = textBoxStyle;
        }

        private static void EditingElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Contract.Requires(sender != null);

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
                var parent = (FrameworkElement)editingElement.Parent;
                if (parent == null)
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
            if (dataGrid == null)
                return false;

            var selectedCells = dataGrid.SelectedCells;
            if (selectedCells == null)
                return false;

            selectedCells = selectedCells
                .Where(c => c.Column.Visibility == Visibility.Visible)
                .ToArray();

            if (!selectedCells.Any())
                return false;

            var visibleColumnIndexes = dataGrid.Columns
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => c.DisplayIndex)
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
        public static IList<IList<string>> GetCellSelection(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            var selectedCells = dataGrid.SelectedCells;
            if (selectedCells == null)
                return null;

            selectedCells = selectedCells
                .Where(c => c.Column.Visibility == Visibility.Visible)
                .ToArray();

            if (!selectedCells.Any())
                return null;

            var orderedRows = selectedCells
                .GroupBy(i => i.Item)
                .OrderBy(i => dataGrid.Items.IndexOf(i.Key));

            return orderedRows.Select(GetRowContent).ToArray();
        }

        /// <summary>
        /// Replaces the selected cells with the data. Cell selection and the data table must have matching dimensions, either 1:n, n:1 or n:n.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> if the dimensions of data and cell selection did match and the cells data has been replaced; otherwise <c>false</c>.</returns>
        public static bool PasteCells(this DataGrid dataGrid, IList<IList<string>> data)
        {
            Contract.Requires(dataGrid != null);
            Contract.Requires(data != null);
            Contract.Requires(Contract.ForAll(data, item => item != null));

            var numberOfRows = data.Count;
            if (data.Count < 1)
                return false;

            var firstRow = data[0];
            Contract.Assume(firstRow != null);
            Contract.Assume(Contract.ForAll(firstRow, item => item != null));

            var numberOfColumns = firstRow.Count;

            var selectedCells = dataGrid.SelectedCells;
            if (selectedCells == null)
                return false;

            selectedCells = selectedCells
                .Where(c => c.IsValid && (c.Column.Visibility == Visibility.Visible))
                .ToArray();

            if (!selectedCells.Any())
                return false;

            var selectedColumns = selectedCells
                .Select(cellInfo => cellInfo.Column)
                .Distinct()
                .ToArray();

            var selectedItems = selectedCells
                .Select(cellInfo => cellInfo.Item)
                .Distinct()
                .ToArray();

            if ((selectedColumns.Length == 1) && (selectedItems.Length == 1))
            {
                // n:1 => n:n
                var selectedColumn = selectedColumns[0];
                selectedColumns = dataGrid.Columns
                    .Where(col => col.DisplayIndex >= selectedColumn.DisplayIndex)
                    .OrderBy(col => col.DisplayIndex)
                    .Where(col => col.Visibility == Visibility.Visible)
                    .Take(numberOfColumns)
                    .ToArray();

                var selectedItem = selectedItems[0];
                selectedItems = dataGrid.Items
                    .Cast<object>()
                    .Skip(dataGrid.Items.IndexOf(selectedItem))
                    .Take(numberOfRows)
                    .ToArray();
            }

            if ((selectedItems.Length == numberOfRows) && (selectedColumns.Length == numberOfColumns))
            {
                // n:n
                Enumerate.AsTuples(selectedItems, data)
                    .ForEach(row => Enumerate.AsTuples(selectedColumns, row.Item2)
                        .ForEach(column => column.Item1.OnPastingCellClipboardContent(row.Item1, column.Item2)));

                return true;
            }

            if ((data.Count == 1) && (firstRow.Count == 1))
            {
                // 1:n
                var cellContent = firstRow[0];

                selectedItems.ForEach(row => selectedColumns
                    .ForEach(column => column.OnPastingCellClipboardContent(row, cellContent)));

                return true;
            }

            return false;
        }

        private static IList<string> GetRowContent(IGrouping<object, DataGridCellInfo> row)
        {
            Contract.Requires(row != null);
            Contract.Ensures(Contract.Result<IList<string>>() != null);

            return row
                .OrderBy(i => i.Column.DisplayIndex)
                .Select(i => i.Column.OnCopyingCellClipboardContent(i.Item))
                .Select(i => i == null ? string.Empty : i.ToString())
                .ToArray();
        }

        #endregion
    }
}
