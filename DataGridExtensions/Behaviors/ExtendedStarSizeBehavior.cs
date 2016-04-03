namespace DataGridExtensions.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using System.Windows.Threading;
    using DataGridExtensions.Framework;

    /// <summary>
    /// Extended start size column behavior. Allows columns to get larger than the client area, but not smaller.
    /// The Resizing behavior can be modified using Ctrl or Shift keys: Ctrl resizes all columns to the right proportionally, Shift fits all columns to the right into the client area.
    /// A tool tip can be attached to the column headers resizing grippers to help the user with these features.
    /// </summary>
    public class ExtendedStarSizeBehavior : Behavior<DataGrid>
    {
        private static readonly DependencyPropertyDescriptor ViewportWidthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ScrollViewer.ViewportWidthProperty, typeof(ScrollViewer));
        private static readonly DependencyPropertyDescriptor NonFrozenColumnsViewportHorizontalOffsetPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGrid.NonFrozenColumnsViewportHorizontalOffsetProperty, typeof(DataGrid));

        private readonly DispatcherThrottle _updateColumnGripperToolTipVisibilityThrottle;

        private int _changingGridSizeCounter;
        private ScrollViewer _scrollViewer;
        private bool _columnsAreFitWithinViewPort;

        /// <summary>
        /// The resrouce key for the default column header gripper tool tip style.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderGripperToolTipStyleKey = new ComponentResourceKey(typeof(ExtendedStarSizeBehavior), "ColumnHeaderGripperToolTipStyle");

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedStarSizeBehavior"/> class.
        /// </summary>
        public ExtendedStarSizeBehavior()
        {
            _updateColumnGripperToolTipVisibilityThrottle = new DispatcherThrottle(DispatcherPriority.Background, UpdateColumnGripperToolTipVisibility);
        }

        /// <summary>
        /// Gets or sets the style of the tool tip for the grippers in the column headers.
        /// </summary>
        public Style ColumnHeaderGripperToolTipStyle
        {
            get { return (Style)GetValue(ColumnHeaderGripperToolTipStyleProperty); }
            set { SetValue(ColumnHeaderGripperToolTipStyleProperty, value); }
        }
        /// <summary>
        /// Identifies the ColumnHeaderGripperToolTipStyle dependency property
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderGripperToolTipStyleProperty =
            DependencyProperty.Register("ColumnHeaderGripperToolTipStyle", typeof(Style), typeof(ExtendedStarSizeBehavior));

        /// <summary>
        /// Gets or sets the resource locator.
        /// </summary>
        public IResourceLocator ResourceLocator
        {
            get { return (IResourceLocator)GetValue(ResourceLocatorProperty); }
            set { SetValue(ResourceLocatorProperty, value); }
        }
        /// <summary>
        /// Identifies the <see cref="ResourceLocator"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ResourceLocatorProperty =
            DependencyProperty.Register("ResourceLocator", typeof(IResourceLocator), typeof(ExtendedStarSizeBehavior));

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the AssociatedObject.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            var dataGrid = AssociatedObject;
            Contract.Assume(dataGrid != null);

            dataGrid.Loaded += DataGrid_Loaded;
            dataGrid.Unloaded += DataGrid_Unloaded;
        }

        private void DataGrid_Loaded(object sender, EventArgs e)
        {
            Contract.Requires(sender != null);

            var dataGrid = (DataGrid)sender;

            dataGrid.BeginInvoke(DispatcherPriority.Background, () => DataGrid_Loaded(dataGrid));
        }

        private void DataGrid_Loaded(DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            _scrollViewer = dataGrid.Template?.FindName("DG_ScrollViewer", dataGrid) as ScrollViewer;
            if (_scrollViewer == null)
                return;

            ViewportWidthPropertyDescriptor.AddValueChanged(_scrollViewer, ScrollViewer_ViewportWidthChanged);
            NonFrozenColumnsViewportHorizontalOffsetPropertyDescriptor.AddValueChanged(dataGrid, DataGrid_NonFrozenColumnsViewportHorizontalOffsetChanged);

            var dataGridEvents = dataGrid.GetAdditionalEvents();

            dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
            dataGridEvents.ColumnVisibilityChanged += DataGrid_ColumnVisibilityChanged;
            dataGridEvents.ColumnActualWidthChanged += DataGrid_ColumnActualWidthChanged;
            dataGridEvents.ColumnDisplayIndexChanged += DataGrid_ColumnDisplayIndexChanged;

            HijackStarSizeColumnsInfo(dataGrid);
            UpdateColumnWidths(dataGrid, null, UpdateMode.ResetStarSize);
            InjectColumnHeaderStyle(dataGrid);
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            Contract.Requires(sender != null);

            var dataGrid = (DataGrid)sender;

            var scrollViewer = _scrollViewer;
            if (scrollViewer == null)
                return;

            ViewportWidthPropertyDescriptor.RemoveValueChanged(scrollViewer, ScrollViewer_ViewportWidthChanged);
            NonFrozenColumnsViewportHorizontalOffsetPropertyDescriptor.RemoveValueChanged(dataGrid, DataGrid_NonFrozenColumnsViewportHorizontalOffsetChanged);

            var dataGridEvents = dataGrid.GetAdditionalEvents();

            dataGrid.Columns.CollectionChanged -= Columns_CollectionChanged;
            dataGridEvents.ColumnVisibilityChanged -= DataGrid_ColumnVisibilityChanged;
            dataGridEvents.ColumnActualWidthChanged -= DataGrid_ColumnActualWidthChanged;
            dataGridEvents.ColumnDisplayIndexChanged -= DataGrid_ColumnDisplayIndexChanged;
        }

        private void DataGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            HijackStarSizeColumnsInfo(dataGrid);
            UpdateColumnWidths(dataGrid, null, (e.Action == NotifyCollectionChangedAction.Add) ? UpdateMode.ResetStarSize : UpdateMode.Default);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void DataGrid_NonFrozenColumnsViewportHorizontalOffsetChanged(object sender, EventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            if (dataGrid == null)
                return;

            if (_changingGridSizeCounter > 0)
                return;

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void ScrollViewer_ViewportWidthChanged(object sender, EventArgs e)
        {
            _changingGridSizeCounter += 1;

            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            dataGrid.BeginInvoke(() =>
            {
                UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
                _changingGridSizeCounter -= 1;
            });
        }

        private void DataGrid_ColumnActualWidthChanged(object sender, DataGridColumnEventArgs e)
        {
            var colum = e.Column;
            if (colum == null)
                return;

            var dataGrid = (DataGrid)sender;
            if (dataGrid == null)
                return;

            if (_changingGridSizeCounter > 0)
                return;

            if (colum.DisplayIndex < dataGrid.FrozenColumnCount)
                return; // wait for NonFrozenColumnsViewportHorizontalOffset change

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, colum, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void DataGrid_ColumnVisibilityChanged(object sender, EventArgs e)
        {
            Contract.Requires(sender != null);

            UpdateColumnWidths((DataGrid)sender, null, UpdateMode.ResetStarSize);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void UpdateColumnWidths(DataGrid dataGrid, DataGridColumn modifiedColum, UpdateMode updateMode)
        {
            Contract.Requires(dataGrid != null);

            var dataGridColumns = dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .Skip(dataGrid.FrozenColumnCount)
                .Where(c => (c.Visibility == Visibility.Visible))
                .ToArray();

            _columnsAreFitWithinViewPort = !ApplyStarSize(dataGridColumns, modifiedColum) && DistributeAvailableSize(dataGrid, dataGridColumns, modifiedColum, updateMode);
        }

        private static bool ApplyStarSize(IEnumerable<DataGridColumn> dataGridColumns, DataGridColumn modifiedColum)
        {
            if ((modifiedColum == null) || (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
                return false;

            var starSize = GetStarSize(modifiedColum);
            if (starSize < double.Epsilon)
                return false;

            var starFactor = modifiedColum.ActualWidth / starSize;

            foreach (var column in dataGridColumns.SkipWhile(c => !Equals(c, modifiedColum)).Skip(1))
            {
                starSize = GetStarSize(column);
                if (starSize > double.Epsilon)
                {
                    column.Width = starSize * starFactor;
                }
            }

            return true;
        }

        private bool DistributeAvailableSize(DataGrid dataGrid, DataGridColumn[] dataGridColumns, DataGridColumn modifiedColum, UpdateMode updateMode)
        {
            Contract.Requires(dataGrid != null);
            Contract.Requires(dataGridColumns != null);

            var scrollViewer = _scrollViewer;
            if (scrollViewer == null)
                return false;

            var startColumnIndex = (modifiedColum != null) ? modifiedColum.DisplayIndex : 0;

            Func<DataGridColumn, bool> isFixedColumn = c => (GetStarSize(c) <= double.Epsilon) || (c.DisplayIndex <= startColumnIndex);
            Func<DataGridColumn, bool> isVariableColumn = c => !isFixedColumn(c);

            var fixedColumnsWidth = dataGridColumns
                .Where(isFixedColumn)
                .Select(c => c.ActualWidth)
                .Sum();

            var getEffectiveColumnSize = (updateMode == UpdateMode.ResetStarSize) ? (Func<DataGridColumn, double>)GetStarSize : GetActualWidth;

            var variableColumnWidths = dataGridColumns
                .Where(isVariableColumn)
                .Select(getEffectiveColumnSize)
                .Sum();

            var availableWidth = scrollViewer.ViewportWidth - dataGrid.CellsPanelHorizontalOffset;
            var nonFrozenColumnsOffset = dataGrid.NonFrozenColumnsViewportHorizontalOffset;
            var spaceAvailable = availableWidth - nonFrozenColumnsOffset - fixedColumnsWidth;

            var allowShrink = (updateMode == UpdateMode.ResetStarSize)
                || (_columnsAreFitWithinViewPort && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));

            if ((!(variableColumnWidths < spaceAvailable)) && !allowShrink)
                return false;

            var factor = spaceAvailable / variableColumnWidths;

            foreach (var column in dataGridColumns.Where(isVariableColumn))
            {
                column.Width = getEffectiveColumnSize(column) * factor;
            }

            return true;
        }

        private static void HijackStarSizeColumnsInfo(DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            foreach (var column in dataGrid.Columns)
            {
                var width = column.Width;
                if (!width.IsStar)
                    continue;

                column.SetValue(StarSizeProperty, width.Value);
                column.Width = column.ActualWidth;
            }
        }

        private void UpdateColumnGripperToolTipVisibility()
        {
            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            DataGridColumn leftColumn = null;
            var isLeftColumnStarSized = false;

            foreach (var column in dataGrid.Columns.OrderBy(c => c.DisplayIndex))
            {
                var leftColumnRightGripperToolTip = (ToolTip)leftColumn?.GetValue(RightGripperToolTipProperty);
                var thisColumnLeftGripperToolTip = (ToolTip)column?.GetValue(LeftGripperToolTipProperty);

                var visibility = isLeftColumnStarSized ? Visibility.Visible : Visibility.Collapsed;

                leftColumnRightGripperToolTip?.Do(t => t.Visibility = visibility);
                thisColumnLeftGripperToolTip?.Do(t => t.Visibility = visibility);

                leftColumn = column;
                isLeftColumnStarSized = GetStarSize(column) > double.Epsilon;
            }

            (leftColumn?.GetValue(RightGripperToolTipProperty) as ToolTip)?.Do(t => t.Visibility = Visibility.Collapsed);
        }

        private void InjectColumnHeaderStyle(DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);

            var baseStyle = dataGrid.ColumnHeaderStyle ?? (Style)dataGrid.FindResource(typeof(DataGridColumnHeader));

            Contract.Assume(baseStyle != null);

            if (baseStyle.Setters.OfType<Setter>().Any(setter => setter.Property == ColumnHeaderGripperExtenderProperty))
                return;

            var newStyle = new Style(typeof(DataGridColumnHeader), baseStyle);
            newStyle.Setters.Add(new Setter(ColumnHeaderGripperExtenderProperty, this));
            dataGrid.ColumnHeaderStyle = newStyle;
        }

        private static double GetActualWidth(DataGridColumn column)
        {
            Contract.Requires(column != null);

            return (double)column.ActualWidth;
        }

        private static double GetStarSize(DataGridColumn column)
        {
            Contract.Requires(column != null);

            return column.GetValue<double>(StarSizeProperty);
        }

        private static readonly DependencyProperty StarSizeProperty =
            DependencyProperty.RegisterAttached("StarSize", typeof(double), typeof(ExtendedStarSizeBehavior), new FrameworkPropertyMetadata(0.0));

        private static readonly DependencyProperty LeftGripperToolTipProperty =
            DependencyProperty.RegisterAttached("LeftGripperToolTip", typeof(ToolTip), typeof(ExtendedStarSizeBehavior));

        private static readonly DependencyProperty RightGripperToolTipProperty =
            DependencyProperty.RegisterAttached("RightGripperToolTip", typeof(ToolTip), typeof(ExtendedStarSizeBehavior));

        private static readonly DependencyProperty ColumnHeaderGripperExtenderProperty =
            DependencyProperty.RegisterAttached("ColumnHeaderGripperExtender", typeof(ExtendedStarSizeBehavior), typeof(ExtendedStarSizeBehavior), new FrameworkPropertyMetadata(null, ColumnHeaderGripperExtender_Changed));

        private static void ColumnHeaderGripperExtender_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnHeader = d as DataGridColumnHeader;
            if (columnHeader == null)
                return;

            var self = e.NewValue as ExtendedStarSizeBehavior;
            if (self == null)
                return;

            var dataGrid = self.AssociatedObject;
            if (dataGrid == null)
                return;

            dataGrid.BeginInvoke(DispatcherPriority.Background, () =>
            {
                self.ApplyGripperToolTip(columnHeader, @"PART_LeftHeaderGripper", LeftGripperToolTipProperty);
                self.ApplyGripperToolTip(columnHeader, @"PART_RightHeaderGripper", RightGripperToolTipProperty);
            });
        }

        private void ApplyGripperToolTip(DataGridColumnHeader columnHeader, string gripperName, DependencyProperty toolTipProperty)
        {
            Contract.Requires(columnHeader != null);

            var gripper = columnHeader.Template?.FindName(gripperName, columnHeader) as Thumb;
            if (gripper == null)
                return;

            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            var style = ColumnHeaderGripperToolTipStyle 
                ?? (ResourceLocator?.FindResource(dataGrid, ColumnHeaderGripperToolTipStyleKey) 
                    ?? dataGrid.TryFindResource(ColumnHeaderGripperToolTipStyleKey)) as Style;

            var toolTip = new ToolTip { Style = style };

            BindingOperations.SetBinding(toolTip, StarSizeProperty, new Binding { Path = new PropertyPath(StarSizeProperty), Source = columnHeader.Column });

            gripper.ToolTip = toolTip;

            columnHeader.Column?.SetValue(toolTipProperty, toolTip);

            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private enum UpdateMode
        {
            Default,
            ResetStarSize
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_updateColumnGripperToolTipVisibilityThrottle != null);
        }
    }
}
