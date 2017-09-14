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

    using JetBrains.Annotations;

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
        public static void EnableMultilineEditing([NotNull] this DataGridTextColumn column)
        {
            Contract.Requires(column != null);

            var textBoxStyle = new Style(typeof(TextBox), column.EditingElementStyle);
            var setters = textBoxStyle.Setters;

            // ReSharper disable once AssignNullToNotNullAttribute
            setters.Add(new EventSetter(UIElement.PreviewKeyDownEvent, (KeyEventHandler)EditingElement_PreviewKeyDown));
            // ReSharper disable once AssignNullToNotNullAttribute
            setters.Add(new Setter(TextBoxBase.AcceptsReturnProperty, true));

            textBoxStyle.Seal();

            column.EditingElementStyle = textBoxStyle;
        }

        private static void EditingElement_PreviewKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
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

                // ReSharper disable once AssignNullToNotNullAttribute
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
        public static bool HasRectangularCellSelection([NotNull] this DataGrid dataGrid)
        {
            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if (selectedCells == null || !selectedCells.Any())
                return false;

            var visibleColumnIndexes = dataGrid.Columns
                // ReSharper disable once PossibleNullReferenceException
                .Where(c => c.Visibility == Visibility.Visible)
                .Select(c => c.DisplayIndex)
                .OrderBy(displayIndex => displayIndex)
                .ToList();

            var rowIndexes = selectedCells
                .Select(cell => cell.Item)
                .Distinct()
                // ReSharper disable once AssignNullToNotNullAttribute
                .Select(item => dataGrid.Items.IndexOf(item))
                .ToArray();

            var columnIndexes = selectedCells
                // ReSharper disable once PossibleNullReferenceException
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
        [CanBeNull, ItemNotNull]
        public static IList<IList<string>> GetCellSelection([NotNull] this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
                return null;

            var orderedRows = selectedCells
                .GroupBy(i => i.Item)
                // ReSharper disable once AssignNullToNotNullAttribute
                // ReSharper disable once PossibleNullReferenceException
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
        public static bool PasteCells([NotNull] this DataGrid dataGrid, [NotNull, ItemNotNull] IList<IList<string>> data)
        {
            Contract.Requires(dataGrid != null);
            Contract.Requires(data != null);
            // Contract.Requires(Contract.ForAll(data, item => item != null));

            var numberOfDataRows = data.Count;
            if (data.Count < 1)
                return false;

            var firstRow = data[0];
            Contract.Assume(firstRow != null);
            // Contract.Assume(Contract.ForAll(firstRow, item => item != null));

            var numberOfDataColumns = firstRow.Count;

            var selectedCells = dataGrid.GetVisibleSelectedCells();

            if ((selectedCells == null) || !selectedCells.Any())
                return false;

            var selectedColumns = selectedCells
                .Select(cellInfo => cellInfo.Column)
                .Distinct()
                // ReSharper disable once PossibleNullReferenceException
                .Where(column => column.Visibility == Visibility.Visible)
                .OrderBy(column => column.DisplayIndex)
                .ToArray();

            var selectedRows = selectedCells
                .Select(cellInfo => cellInfo.Item)
                .Distinct()
                // ReSharper disable once AssignNullToNotNullAttribute
                .OrderBy(item => dataGrid.Items.IndexOf(item))
                .ToArray();

            if ((selectedColumns.Length == 1) && (selectedRows.Length == 1))
            {
                // n:1 => n:n, extend selection to match data
                var selectedColumn = selectedColumns[0];
                selectedColumns = dataGrid.Columns
                    // ReSharper disable PossibleNullReferenceException
                    .Where(col => col.DisplayIndex >= selectedColumn.DisplayIndex)
                    // ReSharper restore PossibleNullReferenceException
                    .OrderBy(col => col.DisplayIndex)
                    .Where(col => col.Visibility == Visibility.Visible)
                    .Take(numberOfDataColumns)
                    .ToArray();

                var selectedItem = selectedRows[0];
                selectedRows = dataGrid.Items
                    .Cast<object>()
                    // ReSharper disable once AssignNullToNotNullAttribute
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
            foreach (var row in Enumerate.AsTuples(selectedRows, Repeat(data, verticalFactor)))
            {
                // ReSharper disable AssignNullToNotNullAttribute, PossibleNullReferenceException
                foreach (var column in Enumerate.AsTuples(selectedColumns, Repeat(row.Item2, horizontalFactor)))
                {
                    column.Item1.OnPastingCellClipboardContent(row.Item1, column.Item2);
                }
                // ReSharper restore AssignNullToNotNullAttribute, PossibleNullReferenceException
            }

            return true;
        }


        /// <summary>
        /// Gets the selected cells that are in visible columns.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The selected cells of visible columns.</returns>
        [CanBeNull]
        public static IList<DataGridCellInfo> GetVisibleSelectedCells([CanBeNull] this DataGrid dataGrid)
        {
            var selectedCells = dataGrid?.SelectedCells;

            return selectedCells?
                .Where(cellInfo => cellInfo.IsValid && (cellInfo.Column != null) && (cellInfo.Column.Visibility == Visibility.Visible))
                .ToArray();
        }

        [NotNull, ItemCanBeNull]
        private static IEnumerable<T> Repeat<T>([NotNull, ItemCanBeNull] ICollection<T> source, int count)
        {
            Contract.Requires(source != null);

            for (var i = 0; i < count; i++)
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        [NotNull, ItemNotNull]
        private static IList<string> GetRowContent([NotNull] IGrouping<object, DataGridCellInfo> row)
        {
            Contract.Requires(row != null);
            Contract.Ensures(Contract.Result<IList<string>>() != null);

            return row
                // ReSharper disable once PossibleNullReferenceException
                .OrderBy(i => i.Column.DisplayIndex)
                .Select(i => i.Column.OnCopyingCellClipboardContent(i.Item))
                .Select(i => i?.ToString() ?? string.Empty)
                .ToArray();
        }

        #endregion
    }
}
