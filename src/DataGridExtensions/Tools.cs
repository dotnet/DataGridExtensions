namespace DataGridExtensions
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using DataGridExtensions.Behaviors;

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
        /// <returns><c>true</c> if 'apply initial sorting is enabled.'</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetApplyInitialSorting(this DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(ApplyInitialSortingProperty);
        }
        /// <summary>
        /// Sets a flag to enable the 'apply initial sorting' feature.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">if set to <c>true</c> the initial sorting will be applied.</param>
        public static void SetApplyInitialSorting(this DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(ApplyInitialSortingProperty, value);
        }
        /// <summary>
        /// Identifies the ApplyInitialSorting dependency property
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// This property is a shortcut for the <see cref="ApplyInitialSortingBehavior"/>.
        /// </summary>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty ApplyInitialSortingProperty =
            DependencyProperty.RegisterAttached("ApplyInitialSorting", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(false, ApplyInitialSortingBehavior.IsEnabled_Changed));

        #endregion

        #region Additional events
        /// <summary>
        /// Gets additional events for a data grid.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The event provider.</returns>
        public static IDataGridEventsProvider GetAdditionalEvents(this DataGrid dataGrid)
        {
            if (!(dataGrid.GetValue(DataGridEventsProviderProperty) is IDataGridEventsProvider eventsProvider))
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
        /// <param name="column">The column.</param>
        /// <returns><c>true</c> if  multi line editing is enabled.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGridTextColumn))]
        public static bool GetIsMultilineEditingEnabled(DataGridTextColumn column)
        {
            return (bool)column.GetValue(IsMultilineEditingEnabledProperty);
        }
        /// <summary>
        /// Sets a value that indicates if multi line editing is enabled for the specified text column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">if set to <c>true</c> multi line editing is enabled.</param>
        public static void SetIsMultilineEditingEnabled(DataGridTextColumn column, bool value)
        {
            column.SetValue(IsMultilineEditingEnabledProperty, value);
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

        #region Force commit on lost focus

        /// <summary>
        /// Gets a value that indicates if a commit will be forced on the data grid when it looses the focus.
        /// </summary>
        /// <param name="dataGrid">The object.</param>
        /// <returns><c>true</c> if a commit will be forced for the data grid when it looses the focus</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetForceCommitOnLostFocus(DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(ForceCommitOnLostFocusProperty);
        }
        /// <summary>
        /// Sets a value that indicates if a commit will be forced on the data grid when it looses the focus.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">If set to <c>true</c> a commit will be forced on the data grid when it looses the focus.</param>
        public static void SetForceCommitOnLostFocus(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(ForceCommitOnLostFocusProperty, value);
        }
        /// <summary>
        /// Identifies the ForceCommitOnLostFocus dependency property
        /// </summary>
        public static readonly DependencyProperty ForceCommitOnLostFocusProperty =
            DependencyProperty.RegisterAttached("ForceCommitOnLostFocus", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(ForceCommitOnLostFocus_Changed));

        private static void ForceCommitOnLostFocus_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;

            dataGrid.PreviewLostKeyboardFocus -= DataGrid_OnPreviewLostKeyboardFocus;

            if (!true.Equals(e.NewValue))
                return;

            dataGrid.PreviewLostKeyboardFocus += DataGrid_OnPreviewLostKeyboardFocus;
        }

        private static void DataGrid_OnPreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (!(e.NewFocus is DependencyObject newFocus))
                return;

            if (!(sender is DataGrid dataGrid))
                return;

            if (newFocus.AncestorsAndSelf().Any(item => ReferenceEquals(item, dataGrid)))
                return; // Focus still in data grid.

            dataGrid.CommitEdit(); // Commit cell
            dataGrid.CommitEdit(); // Commit row
        }

        #endregion  
    }
}