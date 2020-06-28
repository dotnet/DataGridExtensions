namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    /// This class hosts all filter columns and handles the filter changes on the data grid level.
    /// This class will be attached to the DataGrid.
    /// </summary>
    public sealed class DataGridFilterHost
    {
        /// <summary>
        /// Filter information about each column.
        /// </summary>
        private readonly IDictionary<DataGridColumn, DataGridFilterColumnControl?> _filterColumnControls = new Dictionary<DataGridColumn, DataGridFilterColumnControl?>();
        /// <summary>
        /// Timer to defer evaluation of the filter until user has stopped typing.
        /// </summary>
        private DispatcherTimer? _deferFilterEvaluationTimer;
        /// <summary>
        /// The columns that we are currently filtering.
        /// </summary>
        private IEnumerable<DataGridColumn> _filteredColumns = Enumerable.Empty<DataGridColumn>();
        /// <summary>
        /// Flag indicating if filtering is currently enabled.
        /// </summary>
        private bool _isFilteringEnabled;
        /// <summary>
        /// A global filter that is applied in addition to the column filters.
        /// </summary>
        private Predicate<object?>? _globalFilter;

        /// <summary>
        /// Create a new filter host for the given data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid to filter.</param>
        internal DataGridFilterHost(DataGrid dataGrid)
        {
            DataGrid = dataGrid;

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
            dataGrid.Loaded += DataGrid_Loaded;
            dataGrid.CommandBindings.Add(new CommandBinding(DataGrid.SelectAllCommand, DataGrid_SelectAll, DataGrid_CanSelectAll));

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
        public event EventHandler<DataGridFilteringEventArgs>? Filtering;

        /// <summary>
        /// Occurs when any filter has changed.
        /// </summary>
        public event EventHandler? FilterChanged;


        /// <summary>
        /// Clear all existing filter conditions.
        /// </summary>
        public void Clear()
        {
            foreach (var column in _filterColumnControls.Keys)
            {
                column.SetFilter(null);
            }

            EvaluateFilter();
        }

        /// <summary>
        /// Gets a the active filter column controls for this data grid.
        /// </summary>
        public IEnumerable<DataGridFilterColumnControl> FilterColumnControls => _filterColumnControls.Values.Where(item => item != null)!;

        /// <summary>
        /// The data grid this filter is attached to.
        /// </summary>
        public DataGrid DataGrid { get; }

        /// <summary>
        /// Enables filtering by showing or hiding the filter controls.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, filters controls are visible and filtering is enabled.</param>
        internal void Enable(bool value)
        {
            _isFilteringEnabled = value;

            var visibility = value ? Visibility.Visible : Visibility.Hidden;

            foreach (var control in FilterColumnControls)
            {
                control.Visibility = visibility;
            }

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
            DataGrid.CommitEdit(); // Commit cell
            DataGrid.CommitEdit(); // Commit row

            if (_deferFilterEvaluationTimer == null)
            {
                var throttleDelay = DataGrid.GetFilterEvaluationDelay();
                _deferFilterEvaluationTimer = new DispatcherTimer(throttleDelay, DispatcherPriority.Input, (_, __) => EvaluateFilter(), Dispatcher.CurrentDispatcher);
            }

            _deferFilterEvaluationTimer.Restart();
        }

        /// <summary>
        /// Adds a new column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="filterColumnControl"></param>
        internal void AttachColumnControl(DataGridColumn column, DataGridFilterColumnControl filterColumnControl)
        {
            column.SetFilterHost(this);

            filterColumnControl.Visibility = _isFilteringEnabled ? Visibility.Visible : Visibility.Hidden;

            _filterColumnControls[column] = filterColumnControl;
        }

        /// <summary>
        /// Removes an unloaded column.
        /// </summary>
        internal void DetachColumnControl(DataGridColumn column)
        {
            _filterColumnControls[column] = null;
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // To improve keyboard navigation we should not step into the headers filter controls with the TAB key,
            // but only with navigation keys.

            var scrollViewer = DataGrid.Template?.FindName("DG_ScrollViewer", DataGrid) as ScrollViewer;

            var headersPresenter = (FrameworkElement?)scrollViewer?.Template?.FindName("PART_ColumnHeadersPresenter", scrollViewer);

            headersPresenter?.SetValue(KeyboardNavigation.TabNavigationProperty, KeyboardNavigationMode.None);
        }

        private void DataGrid_SelectAll(object? sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            if (!_isFilteringEnabled || (DataGrid.Items.Count > 0))
            {
                if (DataGrid.CanSelectAll())
                {
                    DataGrid.SelectAll();
                }

                return;
            }

            Clear();
        }

        private void DataGrid_CanSelectAll(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = DataGrid.CanSelectAll() || (DataGrid.Items.Count == 0);
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _filterColumnControls.Clear();
                return;
            }

            if (e.OldItems != null)
            {
                foreach (var column in e.OldItems.OfType<DataGridColumn>())
                {
                    _filterColumnControls.Remove(column);
                }
            }

            if (e.NewItems == null)
                return;

            var filteredColumnsWithEmptyHeaderTemplate = e.NewItems
                .OfType<DataGridColumn>()
                .Where(column => column.GetIsFilterVisible() && (column.HeaderTemplate == null))
                .ToArray();

            if (!filteredColumnsWithEmptyHeaderTemplate.Any())
                return;

            var resource = DataGrid.GetResourceLocator()?.FindResource(DataGrid, DataGridFilter.ColumnHeaderTemplateKey)
                ?? DataGrid.TryFindResource(DataGridFilter.ColumnHeaderTemplateKey);

            var headerTemplate = (DataTemplate)resource;

            foreach (var column in filteredColumnsWithEmptyHeaderTemplate)
            {
                column.HeaderTemplate = headerTemplate;
            }
        }

        internal void SetGlobalFilter(Predicate<object?>? globalFilter)
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

            var collectionView = DataGrid.Items;

            // Collect all active filters of all known columns.
            var filteredColumns = GetFilteredColumns();

            if (Filtering != null)
            {
                // Notify client about additional columns being filtered.

                var newColumns = filteredColumns
                    .Except(_filteredColumns)
                    .ToArray();

                if (newColumns.Length > 0)
                {
                    var args = new DataGridFilteringEventArgs(newColumns);
                    Filtering(DataGrid, args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                _filteredColumns = filteredColumns;
            }

            FilterChanged?.Invoke(this, EventArgs.Empty);

            try
            {
                // Apply filter to collection view
                collectionView.Filter = CreatePredicate(filteredColumns);

                // Notify all filters about the change of the collection view.
                foreach (var control in _filterColumnControls.Values)
                {
                    control?.ValuesUpdated();
                }
            }
            catch (InvalidOperationException)
            {
                // InvalidOperation Exception: "'Filter' is not allowed during an AddNew or EditItem transaction."
                // Grid seems to be still in editing mode, even though we have called DataGrid.CommitEdit().
                // Found no way to fix this by code, but after changing the filter another time by typing text it's OK again!
                // Very strange!
            }
        }

        internal Predicate<object?>? CreatePredicate(ICollection<DataGridColumn>? filteredColumns)
        {
            if (filteredColumns?.Any() != true)
            {
                return _globalFilter;
            }

            if (_globalFilter == null)
            {
                return item => filteredColumns.All(filter => filter.Matches(DataGrid, item));
            }

            return item => _globalFilter(item) && filteredColumns.All(filter => filter.Matches(DataGrid, item));
        }

        internal ICollection<DataGridColumn> GetFilteredColumns(DataGridColumn? excluded = null)
        {
            return _filterColumnControls.Keys
                .Where(column => !ReferenceEquals(column, excluded))
                .Where(column => column?.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(column.GetFilter()?.ToString()))
                .ToList()
                .AsReadOnly();
        }
    }
}
