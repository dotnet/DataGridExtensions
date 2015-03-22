namespace DataGridExtensions
{
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Some useful tools for data grids.
    /// </summary>
    public static class Tools
    {
        #region Initial sorting

        /// <summary>
        /// Gets the flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        public static bool GetApplyInitialSorting(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            return (bool)dataGrid.GetValue(ApplyInitialSortingProperty);
        }
        /// <summary>
        /// Sets a flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">if set to <c>true</c> the initial sorting will be appied.</param>
        public static void SetApplyInitialSorting(this DataGrid dataGrid, bool value)
        {
            Contract.Requires(dataGrid != null);

            dataGrid.SetValue(ApplyInitialSortingProperty, value);
        }
        /// <summary>
        /// Identifies the ApplyInitialSorting dependency property
        /// </summary>
        public static readonly DependencyProperty ApplyInitialSortingProperty =
            DependencyProperty.RegisterAttached("ApplyInitialSorting", typeof(bool), typeof(Tools), new UIPropertyMetadata(false, ApplyInitialSorting.IsEnabled_Changed));

        #endregion

        #region Additional events
        /// <summary>
        /// Gets additional events for a data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The event provider.</returns>
        public static IDataGridEventsProvider GetAdditionalEvents(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);
            Contract.Ensures(Contract.Result<IDataGridEventsProvider>() != null);

            var eventsProvider = dataGrid.GetValue(DataGridEventsProviderProperty) as IDataGridEventsProvider;
            if (eventsProvider == null)
            {
                eventsProvider = new DataGridEventsProvider(dataGrid);
                dataGrid.SetValue(DataGridEventsProviderProperty, eventsProvider);
            }

            return eventsProvider;
        }

        /// <summary>
        /// Identifies the DataGridEventsProvider dependency property
        /// </summary>
        private static readonly DependencyProperty DataGridEventsProviderProperty =
            DependencyProperty.RegisterAttached("DataGridEventsProvider", typeof(IDataGridEventsProvider), typeof(Tools));

        #endregion
    }
}
