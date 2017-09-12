namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    /// <summary>
    /// This class hosts all filter columns and handles the filter changes on the data grid level.
    /// This class will be attached to the DataGrid.
    /// </summary>
    public sealed class DataGridFilterHost
    {
        /// <summary>
        /// The data grid this filter is attached to.
        /// </summary>
        [NotNull]
        private readonly DataGrid _dataGrid;
        /// <summary>
        /// Filter information about each column.
        /// </summary>
        [NotNull]
        private readonly List<DataGridFilterColumnControl> _filterColumnControls = new List<DataGridFilterColumnControl>();
        /// <summary>
        /// Timer to defer evaluation of the filter until user has stopped typing.
        /// </summary>
        private DispatcherTimer _deferFilterEvaluationTimer;
        /// <summary>
        /// The columns that we are currently filtering.
        /// </summary>
        private DataGridColumn[] _filteredColumns = new DataGridColumn[0];
        /// <summary>
        /// Flag indicating if filtering is currently enabled.
        /// </summary>
        private bool _isFilteringEnabled;
        /// <summary>
        /// A global filter that is applied in addition to the column filters.
        /// </summary>
        private Predicate<object> _globalFilter;

        /// <summary>
        /// Create a new filter host for the given data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid to filter.</param>
        internal DataGridFilterHost([NotNull] DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            _dataGrid = dataGrid;

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
            dataGrid.Loaded += DataGrid_Loaded;

            if (dataGrid.ColumnHeaderStyle != null)
                return;

            // Assign a default style that changes HorizontalContentAlignment to "Stretch", so our filter symbol will appear on the right edge of the column.
            var baseStyle = (Style)dataGrid.FindResource(typeof(DataGridColumnHeader));
            var newStyle = new Style(typeof(DataGridColumnHeader), baseStyle);
            newStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            dataGrid.ColumnHeaderStyle = newStyle;
        }

        /// <summary>
        /// Occurs before new columns are filtered.
        /// </summary>
        public event EventHandler<DataGridFilteringEventArgs> Filtering;

        /// <summary>
        /// Occurs when any filter has changed.
        /// </summary>
        public event EventHandler FilterChanged;


        /// <summary>
        /// Clear all existing filter conditions.
        /// </summary>
        public void Clear()
        {
            _filterColumnControls.ForEach(filter => filter.Filter = null);
            EvaluateFilter();
        }

        /// <summary>
        /// Gets a the active filter column controls for this data grid.
        /// </summary>
        [NotNull]
        public IList<DataGridFilterColumnControl> FilterColumnControls
        {
            get
            {
                Contract.Ensures(Contract.Result<IList<DataGridFilterColumnControl>>() != null);

                return new ReadOnlyCollection<DataGridFilterColumnControl>(_filterColumnControls);
            }
        }

        /// <summary>
        /// The data grid this filter is attached to.
        /// </summary>
        [NotNull]
        public DataGrid DataGrid
        {
            get
            {
                Contract.Ensures(Contract.Result<DataGrid>() != null);
                return _dataGrid;
            }
        }

        /// <summary>
        /// Enables filtering by showing or hiding the filter controls.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, filters controls are visible and filtering is enabled.</param>
        internal void Enable(bool value)
        {
            _isFilteringEnabled = value;
            _filterColumnControls.ForEach(control => control.Visibility = value ? Visibility.Visible : Visibility.Hidden);
            EvaluateFilter();
        }

        /// <summary>
        /// When any filter condition has changed restart the evaluation timer to defer
        /// the evaluation until the user has stopped typing.
        /// </summary>
        internal void OnFilterChanged()
        {
            if (!_isFilteringEnabled)
                return;

            // Ensure that no cell is in editing state, this would cause an exception when trying to change the filter!
            _dataGrid.CommitEdit(); // Commit cell
            _dataGrid.CommitEdit(); // Commit row

            if (_deferFilterEvaluationTimer == null)
            {
                var throttleDelay = _dataGrid.GetFilterEvaluationDelay();
                _deferFilterEvaluationTimer = new DispatcherTimer(throttleDelay, DispatcherPriority.Input, (_, __) => EvaluateFilter(), Dispatcher.CurrentDispatcher);
            }

            _deferFilterEvaluationTimer.Restart();
        }

        /// <summary>
        /// Adds a new column.
        /// </summary>
        /// <param name="filterColumn"></param>
        internal void AddColumn([NotNull] DataGridFilterColumnControl filterColumn)
        {
            Contract.Requires(filterColumn != null);

            filterColumn.Visibility = _isFilteringEnabled ? Visibility.Visible : Visibility.Hidden;
            _filterColumnControls.Add(filterColumn);
        }

        /// <summary>
        /// Removes an unloaded column.
        /// </summary>
        internal void RemoveColumn([NotNull] DataGridFilterColumnControl filterColumn)
        {
            Contract.Requires(filterColumn != null);

            _filterColumnControls.Remove(filterColumn);
            OnFilterChanged();
        }

        /// <summary>
        /// Creates a new content filter.
        /// </summary>
        [NotNull]
        internal IContentFilter CreateContentFilter(object content)
        {
            Contract.Ensures(Contract.Result<IContentFilter>() != null);

            return _dataGrid.GetContentFilterFactory().Create(content);
        }

        private void DataGrid_Loaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            // To improve keyboard navigation we should not step into the headers filter controls with the TAB key,
            // but only with navigation keys.

            var scrollViewer = DataGrid.Template?.FindName("DG_ScrollViewer", DataGrid) as ScrollViewer;

            var headersPresenter = (FrameworkElement)scrollViewer?.Template?.FindName("PART_ColumnHeadersPresenter", scrollViewer);

            headersPresenter?.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
        }

        private void Columns_CollectionChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;

            var filteredColumnsWithEmptyHeaderTemplate = e.NewItems
                .Cast<DataGridColumn>()
                .Where(column => column.GetIsFilterVisible() && (column.HeaderTemplate == null))
                .ToArray();

            if (!filteredColumnsWithEmptyHeaderTemplate.Any())
                return;

            var resource = _dataGrid.GetResourceLocator()?.FindResource(DataGrid, DataGridFilter.ColumnHeaderTemplateKey) 
                ?? _dataGrid.TryFindResource(DataGridFilter.ColumnHeaderTemplateKey);

            var headerTemplate = (DataTemplate)resource;

            foreach (var column in filteredColumnsWithEmptyHeaderTemplate)
            {
                Contract.Assume(column != null);

                column.HeaderTemplate = headerTemplate;
            }
        }

        internal void SetGlobalFilter(Predicate<object> globalFilter)
        {
            _globalFilter = globalFilter;

            OnFilterChanged();
        }

        /// <summary>
        /// Evaluates the current filters and applies the filtering to the collection view of the items control.
        /// </summary>
        private void EvaluateFilter()
        {
            _deferFilterEvaluationTimer?.Stop();

            var collectionView = _dataGrid.Items;

            // Collect all active filters of all known columns.
            var columnFilters = GetColumnFilters();

            if (Filtering != null)
            {
                // Notify client about additional columns being filtered.
                var columns = columnFilters
                    .Select(filter => filter.Column)
                    .Where(column => column != null)
                    .ToArray();

                var newColumns = columns
                    .Except(_filteredColumns)
                    .ToArray();

                if (newColumns.Length > 0)
                {
                    var args = new DataGridFilteringEventArgs(newColumns);
                    Filtering(_dataGrid, args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                _filteredColumns = columns;
            }

            FilterChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                // Apply filter to collection view
                collectionView.Filter = CreatePredicate(columnFilters);

                // Notify all filters about the change of the collection view.
                _filterColumnControls.ForEach(filter => filter.ValuesUpdated());
            }
            catch (InvalidOperationException)
            {
                // InvalidOperation Exception: "'Filter' is not allowed during an AddNew or EditItem transaction."
                // Grid seems to be still in editing mode, even though we have called DataGrid.CommitEdit().
                // Found no way to fix this by code, but after changing the filter another time by typing text it's OK again!
                // Very strange!
            }
        }

        internal Predicate<object> CreatePredicate(IList<DataGridFilterColumnControl> columnFilters)
        {
            if (columnFilters?.Any() != true)
            {
                return _globalFilter;
            }

            if (_globalFilter == null)
            {
                return item => columnFilters.All(filter => filter.Matches(item));
            }

            return item => _globalFilter(item) && columnFilters.All(filter => filter.Matches(item));
        }

        internal IList<DataGridFilterColumnControl> GetColumnFilters(DataGridFilterColumnControl excluded = null)
        {
            return _filterColumnControls
                .Where(column => !ReferenceEquals(column, excluded))
                .Where(column => column.IsVisible && column.IsFiltered)
                .ToArray();
        }

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_dataGrid != null);
            Contract.Invariant(_filterColumnControls != null);
        }
    }
}
