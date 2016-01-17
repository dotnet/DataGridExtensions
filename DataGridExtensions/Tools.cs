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
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetApplyInitialSorting(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            return dataGrid.GetValue<bool>(ApplyInitialSortingProperty);
        }
        /// <summary>
        /// Sets a flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">if set to <c>true</c> the initial sorting will be applied.</param>
        public static void SetApplyInitialSorting(this DataGrid dataGrid, bool value)
        {
            Contract.Requires(dataGrid != null);

            dataGrid.SetValue(ApplyInitialSortingProperty, value);
        }
        /// <summary>
        /// Identifies the ApplyInitialSorting dependency property
        /// </summary>
        public static readonly DependencyProperty ApplyInitialSortingProperty =
            DependencyProperty.RegisterAttached("ApplyInitialSorting", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(false, ApplyInitialSorting.IsEnabled_Changed));

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

        #region Multi-Line editing
        /// <summary>
        /// Gets a value that indicates if multi line editing is enabled for the specified text column.
        /// </summary>
        /// <param name="obj">The column.</param>
        /// <returns><c>true</c> if  multi line editing is enabled.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGridTextColumn))]
        public static bool GetIsMultilineEditingEnabled(DataGridTextColumn obj)
        {
            Contract.Requires(obj != null);
            return (bool)obj.GetValue(IsMultilineEditingEnabledProperty);
        }
        /// <summary>
        /// Sets a value that indicates if multi line editing is enabled for the specified text column.
        /// </summary>
        /// <param name="obj">The column.</param>
        /// <param name="value">if set to <c>true</c> multi line editing is enabled.</param>
        public static void SetIsMultilineEditingEnabled(DataGridTextColumn obj, bool value)
        {
            Contract.Requires(obj != null);
            obj.SetValue(IsMultilineEditingEnabledProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:DataGridExtensions.Tools.IsMultilineEditingEnabled"/> attached property
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value that indicates if multi line editing is enabled for the specified text column.
        /// </summary>
        /// <remarks>
        /// This property can only be set to true once, toggling the value will have no further effect.
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsMultilineEditingEnabledProperty =
            DependencyProperty.RegisterAttached("IsMultilineEditingEnabled", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(false, IsMultilineEditingEnabled_Changed));

        private static void IsMultilineEditingEnabled_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!true.Equals(e.NewValue))
                return;

            ((DataGridTextColumn)d).EnableMultilineEditing();
        }

        #endregion
    }
}