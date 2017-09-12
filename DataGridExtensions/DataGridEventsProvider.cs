namespace DataGridExtensions
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Windows.Controls;

    using JetBrains.Annotations;

    internal sealed class DataGridEventsProvider : IDataGridEventsProvider
    {
        [NotNull]
        private static readonly IList _emptyList = new object[0];
        // ReSharper disable AssignNullToNotNullAttribute
        [NotNull]
        private static readonly DependencyPropertyDescriptor VisibilityPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.VisibilityProperty, typeof(DataGridColumn));
        [NotNull]
        private static readonly DependencyPropertyDescriptor ActualWidthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
        [NotNull]
        private static readonly DependencyPropertyDescriptor DisplayIndexPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.DisplayIndexProperty, typeof(DataGridColumn));
        // ReSharper restore once AssignNullToNotNullAttribute

        [NotNull]
        private readonly DataGrid _dataGrid;

        public DataGridEventsProvider([NotNull] DataGrid dataGrid)
        {
            _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;

            foreach (var column in dataGrid.Columns)
            {
                AddEventHandlers(column);
            }
        }

        public event EventHandler<DataGridColumnEventArgs> ColumnVisibilityChanged;

        public event EventHandler<DataGridColumnEventArgs> ColumnActualWidthChanged;

        public event EventHandler<DataGridColumnEventArgs> ColumnDisplayIndexChanged;

        private void Columns_CollectionChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (DataGridColumn column in e.NewItems ?? _emptyList)
                    {
                        AddEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (DataGridColumn column in e.OldItems ?? _emptyList)
                    {
                        RemoveEventHandlers(column);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (DataGridColumn column in e.OldItems ?? _emptyList)
                    {
                        RemoveEventHandlers(column);
                    }
                    foreach (DataGridColumn column in e.NewItems ?? _emptyList)
                    {
                        AddEventHandlers(column);
                    }
                    break;
            }
        }

        private void RemoveEventHandlers([NotNull] DataGridColumn column)
        {
            Contract.Requires(column != null);

            VisibilityPropertyDescriptor.RemoveValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.RemoveValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.RemoveValueChanged(column, DataGridColumnDisplayIndex_Changed);
        }

        private void AddEventHandlers([NotNull] DataGridColumn column)
        {
            Contract.Requires(column != null);

            VisibilityPropertyDescriptor.AddValueChanged(column, DataGridColumnVisibility_Changed);
            ActualWidthPropertyDescriptor.AddValueChanged(column, DataGridColumnActualWidth_Changed);
            DisplayIndexPropertyDescriptor.AddValueChanged(column, DataGridColumnDisplayIndex_Changed);
        }

        private void OnColumnVisibilityChanged([NotNull] DataGridColumn column)
        {
            Contract.Requires(column != null);

            ColumnVisibilityChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnActualWidthChanged([NotNull] DataGridColumn column)
        {
            Contract.Requires(column != null);

            ColumnActualWidthChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void OnColumnDisplayIndexChanged([NotNull] DataGridColumn column)
        {
            Contract.Requires(column != null);

            ColumnDisplayIndexChanged?.Invoke(_dataGrid, new DataGridColumnEventArgs(column));
        }

        private void DataGridColumnVisibility_Changed([NotNull] object source, [NotNull] EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnVisibilityChanged((DataGridColumn)source);
        }

        private void DataGridColumnActualWidth_Changed([NotNull] object source, [NotNull] EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnActualWidthChanged((DataGridColumn)source);
        }
        private void DataGridColumnDisplayIndex_Changed([NotNull] object source, [NotNull] EventArgs e)
        {
            Contract.Requires(source != null);

            OnColumnDisplayIndexChanged((DataGridColumn)source);
        }
    }
}
