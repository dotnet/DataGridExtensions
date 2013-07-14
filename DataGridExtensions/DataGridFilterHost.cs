﻿using System;
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
        private readonly DataGrid dataGrid;
        /// <summary>
        /// Filter information about each column.
        /// </summary>
        private readonly List<DataGridFilterColumnControl> filterColumnControls = new List<DataGridFilterColumnControl>();
        /// <summary>
        /// Timer to defer evaluation of the filter until user has stopped typing.
        /// </summary>
        private DispatcherTimer deferFilterEvaluationTimer;
        /// <summary>
        /// The columns that we are currently filtering.
        /// </summary>
        private DataGridColumn[] filteredColumns = new DataGridColumn[0];

        /// <summary>
        /// Create a new filter for the given data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid to filter.</param>
        internal DataGridFilterHost(DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");

            this.dataGrid = dataGrid;

            this.dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;

            if (this.dataGrid.ColumnHeaderStyle != null) 
                return;

            // Assign a default style that changes HorizontalContentAlignment to "Stretch", so our filter symbol will appear on the right edge of the column.
            var baseStyle = (Style)dataGrid.FindResource(typeof(DataGridColumnHeader));
            var newStyle = new Style(typeof(DataGridColumnHeader), baseStyle);
            newStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            this.dataGrid.ColumnHeaderStyle = newStyle;
        }

        /// <summary>
        /// Occurs before new columns are filtered.
        /// </summary>
        public event EventHandler<DataGridFilteringEventArgs> Filtering;


        /// <summary>
        /// Clear all existing filter conditions.
        /// </summary>
        public void Clear()
        {
            filterColumnControls.ForEach(filter => filter.Filter = null);
            EvaluateFilter();
        }

        /// <summary>
        /// Gets a the active filter column controls for this data grid.
        /// </summary>
        public IList<DataGridFilterColumnControl> FilterColumnControls
        {
            get
            {
                return new ReadOnlyCollection<DataGridFilterColumnControl>(filterColumnControls);
            }
        }

        /// <summary>
        /// Enables filtering by showing or hiding the filter contols.
        /// </summary>
        /// <param name="value">if set to <c>true</c>, filters controls are visible and filtering is enabled.</param>
        internal void Enable(bool value)
        {
            filterColumnControls.ForEach(control => control.Visibility = value ? Visibility.Visible : Visibility.Hidden);
            EvaluateFilter();
        }

        /// <summary>
        /// When any filter condition has changed restart the evaluation timer to defer
        /// the evaluation until the user has stopped typing.
        /// </summary>
        internal void FilterChanged()
        {
            if (deferFilterEvaluationTimer == null)
            {
                var throttleDelay = dataGrid.GetFilterEvaluationDelay();
                deferFilterEvaluationTimer = new DispatcherTimer(throttleDelay, DispatcherPriority.Input, (_, __) => EvaluateFilter(), Dispatcher.CurrentDispatcher);
            }

            deferFilterEvaluationTimer.Restart();
        }

        /// <summary>
        /// Adds a new collumn.
        /// </summary>
        /// <param name="filterColumn"></param>
        internal void AddColumn(DataGridFilterColumnControl filterColumn)
        {
            filterColumnControls.Add(filterColumn);
        }

        /// <summary>
        /// Removes an unloaded column.
        /// </summary>
        internal void RemoveColumn(DataGridFilterColumnControl filterColumn)
        {
            filterColumnControls.Remove(filterColumn);
        }

        /// <summary>
        /// Creates a new content filter.
        /// </summary>
        internal IContentFilter CreateContentFilter(object content)
        {
            return this.dataGrid.GetContentFilterFactory().Create(content);
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e == null) || (e.NewItems == null))
                return;

            if (!this.dataGrid.GetIsAutoFilterEnabled())
                return;

            var filteredColumnsWithEmptyHeaderTemplate = e.NewItems.Cast<DataGridColumn>().Where(column => column.GetIsFilterVisible() && (column.HeaderTemplate == null)).ToArray();

            if (!filteredColumnsWithEmptyHeaderTemplate.Any())
                return;

            var headerTemplate = (DataTemplate)this.dataGrid.FindResource(DataGridFilter.ColumnHeaderTemplateKey);

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
            if (deferFilterEvaluationTimer != null)
                deferFilterEvaluationTimer.Stop();

            var collectionView = dataGrid.Items;

            // Collect all active filters of all known columns.
            var filters = filterColumnControls.Where(column => column.IsVisible && column.IsFiltered).ToArray();

            if (Filtering != null)
            {
                // Notify client about additional columns being filtered.
                var columns = filters.Select(filter => filter.Column).Where(column => column != null).ToArray();
                var newColumns = columns.Except(filteredColumns).ToArray();

                if (newColumns.Length > 0)
                {
                    var args = new DataGridFilteringEventArgs(newColumns);
                    Filtering(dataGrid, args);


                    if (args.Cancel)
                    {
                        return;
                    }
                }

                filteredColumns = columns;
            }

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
            filterColumnControls.ForEach(filter => filter.ValuesUpdated());
        }
    }
}