using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DataGridExtensions
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls.Primitives;

    /// <summary>
    /// This class hosts all filter columns and handles the filter changes on the data grid level.
    /// This class will be attached to the DataGrid.
    /// </summary>
    public sealed class DataGridFilterHost
    {
        /// <summary>
        /// The data grid this filter is attached to.
        /// </summary>
        private readonly DataGrid _dataGrid;
        /// <summary>
        /// Filter information about each column.
        /// </summary>
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
        /// Create a new filter host for the given data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid to filter.</param>
        internal DataGridFilterHost(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");

            _dataGrid = dataGrid;

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;

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
        public IList<DataGridFilterColumnControl> FilterColumnControls
        {
            get
            {
                return new ReadOnlyCollection<DataGridFilterColumnControl>(_filterColumnControls);
            }
        }

        /// <summary>
        /// The data grid this filter is attached to.
        /// </summary>
        public DataGrid DataGrid
        {
            get
            {
                return _dataGrid;
            }
        }

        /// <summary>
        /// Enables filtering by showing or hiding the filter contols.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, filters controls are visible and filtering is enabled.</param>
        internal void Enable(bool value)
        {
            _filterColumnControls.ForEach(control => control.Visibility = value ? Visibility.Visible : Visibility.Hidden);
            EvaluateFilter();
        }

        /// <summary>
        /// When any filter condition has changed restart the evaluation timer to defer
        /// the evaluation until the user has stopped typing.
        /// </summary>
        internal void OnFilterChanged()
        {
            // Ensure that no cell is in editing state, this would cause an exception when trying to change the filter!
            _dataGrid.CommitEdit();

            if (_deferFilterEvaluationTimer == null)
            {
                var throttleDelay = _dataGrid.GetFilterEvaluationDelay();
                _deferFilterEvaluationTimer = new DispatcherTimer(throttleDelay, DispatcherPriority.Input, (_, __) => EvaluateFilter(), Dispatcher.CurrentDispatcher);
            }

            _deferFilterEvaluationTimer.Restart();
        }

        /// <summary>
        /// Adds a new collumn.
        /// </summary>
        /// <param name="filterColumn"></param>
        internal void AddColumn(DataGridFilterColumnControl filterColumn)
        {
            _filterColumnControls.Add(filterColumn);
        }

        /// <summary>
        /// Removes an unloaded column.
        /// </summary>
        internal void RemoveColumn(DataGridFilterColumnControl filterColumn)
        {
            _filterColumnControls.Remove(filterColumn);
            OnFilterChanged();
        }

        /// <summary>
        /// Creates a new content filter.
        /// </summary>
        internal IContentFilter CreateContentFilter(object content)
        {
            return _dataGrid.GetContentFilterFactory().Create(content);
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e == null) || (e.NewItems == null))
                return;

            if (!_dataGrid.GetIsAutoFilterEnabled())
                return;

            var filteredColumnsWithEmptyHeaderTemplate = e.NewItems.Cast<DataGridColumn>().Where(column => column.GetIsFilterVisible() && (column.HeaderTemplate == null)).ToArray();

            if (!filteredColumnsWithEmptyHeaderTemplate.Any())
                return;

            var headerTemplate = (DataTemplate)this._dataGrid.FindResource(DataGridFilter.ColumnHeaderTemplateKey);

            foreach (var column in filteredColumnsWithEmptyHeaderTemplate)
            {
                column.HeaderTemplate = headerTemplate;
            }
        }

        /// <summary>
        /// Evaluates the current filters and applies the filtering to the collection view of the items control.
        /// </summary>
        private void EvaluateFilter()
        {
            if (_deferFilterEvaluationTimer != null)
                _deferFilterEvaluationTimer.Stop();

            var collectionView = _dataGrid.Items;

            // Collect all active filters of all known columns.
            var filters = _filterColumnControls.Where(column => column.IsVisible && column.IsFiltered).ToArray();

            if (Filtering != null)
            {
                // Notify client about additional columns being filtered.
                var columns = filters.Select(filter => filter.Column).Where(column => column != null).ToArray();
                var newColumns = columns.Except(_filteredColumns).ToArray();

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

            if (FilterChanged != null)
            {
                FilterChanged(this, EventArgs.Empty);
            }

            try
            {
                // Apply filter to collection view
                if (!filters.Any())
                {
                    collectionView.Filter = null;
                }
                else
                {
                    collectionView.Filter = item => filters.All(filter => filter.Matches(item));
                }

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
    }
}
