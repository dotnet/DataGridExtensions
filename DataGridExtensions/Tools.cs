using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridExtensions
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    public static class Tools
    {
        /// <summary>
        /// Gets the flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        public static bool GetApplyInitialSorting(this DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(ApplyInitialSortingProperty);
        }
        /// <summary>
        /// Sets a flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">if set to <c>true</c> the initial sorting will be appied.</param>
        public static void SetApplyInitialSorting(this DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(ApplyInitialSortingProperty, value);
        }
        /// <summary>
        /// Identifies the ApplyInitialSorting dependency property
        /// </summary>
        public static readonly DependencyProperty ApplyInitialSortingProperty =
            DependencyProperty.RegisterAttached("ApplyInitialSorting", typeof(bool), typeof(Tools), new UIPropertyMetadata(false, ApplyInitialSorting_Changed));

        private static void ApplyInitialSorting_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            if ((bool)e.NewValue)
                dataGrid.Loaded += dataGrid_Loaded;
            else
                dataGrid.Loaded -= dataGrid_Loaded;
        }

        private static void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            foreach (var column in dataGrid.Columns)
            {
                if ((column != null) && (column.SortDirection.HasValue) && !string.IsNullOrEmpty(column.SortMemberPath))
                {
                    dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, column.SortDirection.Value));
                }
            } 
        }            
    }
}
