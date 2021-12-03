namespace DataGridExtensions
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;

    internal sealed class DataGridEventsProvider : IDataGridEventsProvider
    {
        private static readonly DependencyPropertyDescriptor VisibilityPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor ActualWidthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor DisplayIndexPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.DisplayIndexProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor SortDirectionPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.SortDirectionProperty, typeof(DataGridColumn));

        private readonly DataGrid _dataGrid;

        public DataGridEventsProvider(DataGrid dataGrid)
        {
            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;

            foreach (var column in dataGrid.Columns)
            {
                AddEventHandlers(column);
            }
        }

        public event EventHandler<DataGridColumnEventArgs>? ColumnVisibilityChanged;

        public event EventHandler<DataGridColumnEventArgs>? ColumnActualWidthChanged;

        public event EventHandler<DataGridColumnEventArgs>? ColumnDisplayIndexChanged;

        public event EventHandler<DataGridColumnEventArgs>? ColumnSortDirectionChanged;

        private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
#nullable disable
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var column in e.NewItems.OfType<DataGridColumn>())
                    {
                        AddEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var column in e.OldItems.OfType<DataGridColumn>())
                    {
                        RemoveEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var column in e.OldItems.OfType<DataGridColumn>())
                    {
                        RemoveEventHandlers(column);
                    }
                    foreach (var column in e.NewItems.OfType<DataGridColumn>())
                    {
                        AddEventHandlers(column);
                    }
                    break;
#nullable restore
            }
        }

        private void RemoveEventHandlers(DataGridColumn column)
        {
            VisibilityPropertyDescriptor.RemoveValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.RemoveValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.RemoveValueChanged(column, DataGridColumnDisplayIndex_Changed);
            SortDirectionPropertyDescriptor.RemoveValueChanged(column, DataGridColumnSortDirection_Changed);
        }

        private void AddEventHandlers(DataGridColumn column)
        {
            VisibilityPropertyDescriptor.AddValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.AddValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.AddValueChanged(column, DataGridColumnDisplayIndex_Changed);
            SortDirectionPropertyDescriptor.AddValueChanged(column, DataGridColumnSortDirection_Changed);
        }

        private void OnColumnVisibilityChanged(DataGridColumn column)
        {
            ColumnVisibilityChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnActualWidthChanged(DataGridColumn column)
        {
            ColumnActualWidthChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnDisplayIndexChanged(DataGridColumn column)
        {
            ColumnDisplayIndexChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnSortDirectionChanged(DataGridColumn column)
        {
            ColumnSortDirectionChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void DataGridColumnVisibility_Changed(object? source, EventArgs e)
        {
            // ! source is never null
            OnColumnVisibilityChanged((DataGridColumn)source!);
        }

        private void DataGridColumnActualWidth_Changed(object? source, EventArgs e)
        {
            // ! source is never null
            OnColumnActualWidthChanged((DataGridColumn)source!);
        }

        private void DataGridColumnDisplayIndex_Changed(object? source, EventArgs e)
        {
            // ! source is never null
            OnColumnDisplayIndexChanged((DataGridColumn)source!);
        }

        private void DataGridColumnSortDirection_Changed(object? source, EventArgs e)
        {
            // ! source is never null
            OnColumnSortDirectionChanged((DataGridColumn)source!);
        }
    }
}
