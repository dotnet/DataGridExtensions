namespace DataGridExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Controls;

    internal sealed class DataGridEventsProvider : IDataGridEventsProvider
    {
        private static readonly IList EmptyList = new object[0];
        private static readonly DependencyPropertyDescriptor VisibilityPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor ActualWidthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
        private static readonly DependencyPropertyDescriptor DisplayIndexPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.DisplayIndexProperty, typeof(DataGridColumn));

        private readonly DataGrid _dataGrid;

        public DataGridEventsProvider(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");

            _dataGrid = dataGrid;
            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;

            foreach (var column in dataGrid.Columns)
            {
                AddEventHandlers(column);
            }
        }

        public event EventHandler<DataGridColumnEventArgs> ColumnVisibilityChanged;

        public event EventHandler<DataGridColumnEventArgs> ColumnActualWidthChanged;

        public event EventHandler<DataGridColumnEventArgs> ColumnDisplayIndexChanged;

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (DataGridColumn column in e.NewItems ?? EmptyList)
                    {
                        AddEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (DataGridColumn column in e.OldItems ?? EmptyList)
                    {
                        RemoveEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (DataGridColumn column in e.OldItems ?? EmptyList)
                    {
                        RemoveEventHandlers(column);
                    }
                    foreach (DataGridColumn column in e.NewItems ?? EmptyList)
                    {
                        AddEventHandlers(column);
                    }
                    break;
            }
        }

        private void RemoveEventHandlers(DataGridColumn column)
        {
            Contract.Requires(column != null);

            VisibilityPropertyDescriptor.RemoveValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.RemoveValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.RemoveValueChanged(column, DataGridColumnDisplayIndex_Changed);
        }

        private void AddEventHandlers(DataGridColumn column)
        {
            Contract.Requires(column != null);

            VisibilityPropertyDescriptor.AddValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.AddValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.AddValueChanged(column, DataGridColumnDisplayIndex_Changed);
        }

        private void OnColumnVisibilityChanged(DataGridColumn column)
        {
            Contract.Requires(column != null);

            var handler = ColumnVisibilityChanged;
            if (handler != null)
                handler(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnActualWidthChanged(DataGridColumn column)
        {
            Contract.Requires(column != null);

            var handler = ColumnActualWidthChanged;
            if (handler != null)
                handler(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnDisplayIndexChanged(DataGridColumn column)
        {
            Contract.Requires(column != null);

            var handler = ColumnDisplayIndexChanged;
            if (handler != null)
                handler(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void DataGridColumnVisibility_Changed(object source, EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnVisibilityChanged((DataGridColumn)source);
        }

        private void DataGridColumnActualWidth_Changed(object source, EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnActualWidthChanged((DataGridColumn)source);
        }
        private void DataGridColumnDisplayIndex_Changed(object source, EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnDisplayIndexChanged((DataGridColumn)source);
        }
    }
}
