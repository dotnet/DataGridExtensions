namespace DataGridExtensions
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Threading;

    using DataGridExtensions.Behaviors;

    using TomsToolbox.Wpf;

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
            if (dataGrid.GetValue(DataGridEventsProviderProperty) is not IDataGridEventsProvider eventsProvider)
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
            if (e.NewFocus is not DependencyObject newFocus)
                return;

            if (sender is not DataGrid dataGrid)
                return;

            if (newFocus.AncestorsAndSelf().Any(item => ReferenceEquals(item, dataGrid)))
                return; // Focus still in data grid.

            dataGrid.CommitEdit(); // Commit cell
            dataGrid.CommitEdit(); // Commit row
        }

        #endregion

        #region Track focused cell

        /// <summary>
        /// Gets a value that indicates if the last focused cell of the data grid will be tracked.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns><c>true</c> if if the last focused cell of the data grid will be tracked.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetTrackFocusedCell(this DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(TrackFocusedCellProperty);
        }
        /// <summary>
        /// Sets a value that indicates if the last focused cell of the data grid will be tracked.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value"><c>true</c> if if the last focused cell of the data grid should be tracked.</param>
        public static void SetTrackFocusedCell(this DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(TrackFocusedCellProperty, value);
        }
        /// <summary>
        /// Identifies the TrackFocusedCell property.
        /// </summary>
        public static readonly DependencyProperty TrackFocusedCellProperty = DependencyProperty.RegisterAttached(
            "TrackFocusedCell", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(default(bool), TrackFocusedCell_Changed));

        /// <summary>
        /// Gets the cell that has/had the focus; requires <see cref="TrackFocusedCellProperty"/> to be set to true;
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The cell that has/had the focus; <c>null</c> if no cell was recently focused or <see cref="TrackFocusedCellProperty"/> is not true.</returns>
        public static DataGridCell? GetLastFocusedCell(this DataGrid dataGrid)
        {
            return (DataGridCell?)dataGrid.GetValue(LastFocusedCellProperty);
        }
        private static void SetLastFocusedCell(DataGrid dataGrid, DataGridCell value)
        {
            dataGrid.SetValue(LastFocusedCellPropertyKey, value);
        }
        private static readonly DependencyPropertyKey LastFocusedCellPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "LastFocusedCell", typeof(DataGridCell), typeof(Tools), new PropertyMetadata(default(DataGridCell)));
        /// <summary>
        /// Identifies the LastFocusedCell property.
        /// </summary>
        public static readonly DependencyProperty LastFocusedCellProperty = LastFocusedCellPropertyKey.DependencyProperty;

        private static void TrackFocusedCell_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid)
                return;

            if (true.Equals(e.NewValue))
            {
                dataGrid.GotFocus += DataGrid_GotFocus;
            }
            else
            {
                dataGrid.GotFocus -= DataGrid_GotFocus;

            }
        }

        private static void DataGrid_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            if (e.OriginalSource is DataGridCell cell)
            {
                SetLastFocusedCell(dataGrid, cell);
            }
        }

        #endregion

        #region Move focus on navigation key

        /// <summary>
        /// Gets a value that indicates if the focus should move out of the control when the user presses the Up/Down navigation keys.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetMoveFocusOnNavigationKey(UIElement element)
        {
            return (bool)element.GetValue(MoveFocusOnNavigationKeyProperty);
        }
        /// <summary>
        /// Sets a value that indicates if the focus should move out of the control when the user presses the Up/Down navigation keys.
        /// </summary>
        public static void SetMoveFocusOnNavigationKey(UIElement element, bool value)
        {
            element.SetValue(MoveFocusOnNavigationKeyProperty, value);
        }
        /// <summary>
        /// Identifies the MoveFocusOnNavigationKey property.
        /// </summary>
        public static readonly DependencyProperty MoveFocusOnNavigationKeyProperty = DependencyProperty.RegisterAttached(
            "MoveFocusOnNavigationKey", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(default(bool), MoveFocusOnNavigationKey_Changed));

        private static void MoveFocusOnNavigationKey_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (true.Equals(e.NewValue))
                {
                    element.PreviewKeyDown += MoveFocusOnNavigationKey_KeyDown;
                }
                else
                {
                    element.PreviewKeyDown -= MoveFocusOnNavigationKey_KeyDown;
                }
            }

        }

        private static void MoveFocusOnNavigationKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                if (sender is UIElement element)
                {
                    e.Handled = true;
                    element.BeginInvoke(() => element.MoveFocus(new TraversalRequest(e.Key == Key.Down ? FocusNavigationDirection.Down : FocusNavigationDirection.Up)));
                }
            }
        }

        #endregion

        #region Move focus to data grid on navigation key

        /// <summary>
        /// Gets a value that indicates if the focus should move out of the control when the user presses the Up/Down navigation keys.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetMoveFocusToDataGridOnNavigationKey(UIElement element)
        {
            return (bool)element.GetValue(MoveFocusOnNavigationKeyProperty);
        }
        /// <summary>
        /// Sets a value that indicates if the focus should move out of the control when the user presses the Up/Down navigation keys.
        /// </summary>
        public static void SetMoveFocusToDataGridOnNavigationKey(UIElement element, bool value)
        {
            element.SetValue(MoveFocusOnNavigationKeyProperty, value);
        }
        /// <summary>
        /// Identifies the MoveFocusToDataGridOnNavigationKey property.
        /// </summary>
        public static readonly DependencyProperty MoveFocusToDataGridOnNavigationKeyProperty = DependencyProperty.RegisterAttached(
            "MoveFocusToDataGridOnNavigationKey", typeof(bool), typeof(Tools), new FrameworkPropertyMetadata(default(bool), MoveFocusToDataGridOnNavigationKey_Changed));

        private static void MoveFocusToDataGridOnNavigationKey_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (true.Equals(e.NewValue))
                {
                    element.PreviewKeyDown += MoveFocusToDataGridOnNavigationKey_KeyDown;
                }
                else
                {
                    element.PreviewKeyDown -= MoveFocusToDataGridOnNavigationKey_KeyDown;
                }
            }

        }

        private static void MoveFocusToDataGridOnNavigationKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up)
            {
                if (sender is UIElement element)
                {
                    e.Handled = true;
                    var dataGrid = element.TryFindAncestorOrSelf<DataGrid>();
                    if (dataGrid != null)
                    {
                        MoveFocusToDataGrid(element, dataGrid);
                    }
                    else
                    {
                        element.BeginInvoke(() => element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down)));
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Tries to move the focus from the origin element to the data grid control.
        /// </summary>
        /// <param name="origin">The element that starts this request-</param>
        /// <param name="target">The data grid that should receive the focus.</param>
        public static void MoveFocusToDataGrid(this UIElement origin, DataGrid? target)
        {
            var lastFocusedCell = target?.GetLastFocusedCell();

            origin.BeginInvoke(DispatcherPriority.Background, () =>
            {
                if (origin.IsKeyboardFocusWithin)
                {
                    if (lastFocusedCell?.IsLoaded == true)
                    {
                        lastFocusedCell.Focus();
                    }
                    else
                    {
                        var focusedElement = Keyboard.FocusedElement;
                        (focusedElement as UIElement)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    }
                }
            });
        }
    }
}
