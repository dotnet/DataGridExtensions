namespace DataGridExtensionsSample.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Windows.Controls;
    using System.Windows.Input;

    using DataGridExtensions;

    using DataGridExtensionsSample.Infrastructure;

    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    /// <summary>
    /// Provides a view model for paginated data display, supporting sorting and filtering operations within a data grid.
    /// This behavior is enabled by implementing the ICustomFilter interface, allowing the view model to respond to changes in sorting and filtering criteria.
    /// </summary>
    /// <remarks>This class manages the retrieval and presentation of data items in discrete chunks, allowing
    /// users to navigate through large datasets efficiently. Sorting and filtering criteria can be applied dynamically,
    /// and the view model updates the displayed items accordingly. Intended for use in UI scenarios where data paging
    /// is required. Thread safety is not guaranteed; interactions should occur on the UI thread.</remarks>
    [VisualCompositionExport(RegionId.Main, Sequence = 10)]
    [DisplayName("Data Paging")]
    internal sealed class PagingViewModel : ICustomFilter
    {
        private readonly IList<DataItem> _database = new DataProvider().Items;

        private IEnumerator<DataItem>? _cursor;
        private IReadOnlyCollection<SortDescription>? _activeSortDescriptions;
        private Dictionary<string, string>? _filterDescriptions;

        public ObservableCollection<DataItem> Items { get; } = new();

        public ICommand TakeNextChunkCommand => new DelegateCommand(CanTakeNextChunk, TakNextChunk);

        public PagingViewModel()
        {
            ResetCursor();
        }

        public void OnSortChanged(DataGrid dataGrid, IReadOnlyCollection<SortDescription> sortDescriptions)
        {
            _activeSortDescriptions = sortDescriptions;

            ResetCursor();
        }

        public void OnFilterChanged(DataGrid dataGrid, IReadOnlyCollection<DataGridColumn> dataGridColumns)
        {
            _filterDescriptions = new();

            foreach (var column in dataGridColumns)
            {
                var sortMemberPath = column.SortMemberPath;
                if (string.IsNullOrEmpty(sortMemberPath))
                    continue;

                var value = column.GetFilter()?.ToString();
                if (string.IsNullOrEmpty(value))
                    continue;

                _filterDescriptions.Add(sortMemberPath, value);
            }

            ResetCursor();
        }

        public bool DisableMultipleColumnSorting => false;

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private void ResetCursor()
        {
            Items.Clear();

            _cursor?.Dispose();

            var query = _database.AsQueryable();

            if (_filterDescriptions != null)
            {
                foreach (var (propertyName, filterValue) in _filterDescriptions)
                {
                    var parameter = Expression.Parameter(typeof(DataItem), "item");
                    var property = Expression.Property(parameter, propertyName);
                    var toString = Expression.Call(property, "ToString", null);
                    var contains = Expression.Call(toString, nameof(string.Contains), null, Expression.Constant(filterValue), Expression.Constant(StringComparison.OrdinalIgnoreCase));
                    var lambda = Expression.Lambda<Func<DataItem, bool>>(contains, parameter);

                    query = query.Where(lambda);
                }
            }

            if (_activeSortDescriptions != null)
            {
                IOrderedQueryable<DataItem>? orderedQuery = null;

                foreach (var sortDesc in _activeSortDescriptions)
                {
                    var propertyName = sortDesc.PropertyName;
                    var direction = sortDesc.Direction;

                    var keySelector = CreatePropertySelector(propertyName);

                    if (orderedQuery == null)
                    {
                        orderedQuery = direction == ListSortDirection.Ascending
                            ? query.OrderBy(keySelector)
                            : query.OrderByDescending(keySelector);
                    }
                    else
                    {
                        orderedQuery = direction == ListSortDirection.Ascending
                            ? orderedQuery.ThenBy(keySelector)
                            : orderedQuery.ThenByDescending(keySelector);
                    }
                }

                if (orderedQuery != null)
                {
                    query = orderedQuery;
                }
            }

            _cursor = query.GetEnumerator();

            TakNextChunk();
        }

        private void TakNextChunk()
        {
            for (var i = 0; i < 10; i++)
            {
                if (!_cursor!.MoveNext())
                {
                    _cursor.Dispose();
                    _cursor = null;
                    break;
                }
                Items.Add(_cursor.Current);
            }
        }

        private bool CanTakeNextChunk()
        {
            return _cursor != null;
        }

        private static Expression<Func<DataItem, object>> CreatePropertySelector(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(DataItem), "item");
            var property = Expression.Property(parameter, propertyName);
            var converted = Expression.Convert(property, typeof(object));
            return Expression.Lambda<Func<DataItem, object>>(converted, parameter);
        }
    }
}

