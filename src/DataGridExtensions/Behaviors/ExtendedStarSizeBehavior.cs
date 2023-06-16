namespace DataGridExtensions.Behaviors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;

    using DataGridExtensions.Framework;

    using Microsoft.Xaml.Behaviors;

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

        private ScrollViewer? _scrollViewer;

        private int _changingGridSizeCounter;
        private bool _columnsAreFitWithinViewPort;

        /// <summary>
        /// The resource key for the default column header gripper tool tip style.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderGripperToolTipStyleKey = new ComponentResourceKey(typeof(ExtendedStarSizeBehavior), "ColumnHeaderGripperToolTipStyle");

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedStarSizeBehavior" /> class.
        /// </summary>
        public ExtendedStarSizeBehavior()
        {
            _updateColumnGripperToolTipVisibilityThrottle = new DispatcherThrottle(DispatcherPriority.Background, UpdateColumnGripperToolTipVisibility);
        }

        /// <summary>
        /// Gets or sets the style of the tool tip for the grippers in the column headers.
        /// </summary>
        public Style? ColumnHeaderGripperToolTipStyle
        {
            get => (Style)GetValue(ColumnHeaderGripperToolTipStyleProperty);
            set => SetValue(ColumnHeaderGripperToolTipStyleProperty, value);
        }
        /// <summary>
        /// Identifies the ColumnHeaderGripperToolTipStyle dependency property
        /// </summary>
        public static readonly DependencyProperty ColumnHeaderGripperToolTipStyleProperty =
            DependencyProperty.Register(nameof(ColumnHeaderGripperToolTipStyle), typeof(Style), typeof(ExtendedStarSizeBehavior));

        /// <summary>
        /// Gets or sets the resource locator.
        /// </summary>
        public IResourceLocator? ResourceLocator
        {
            get => (IResourceLocator)GetValue(ResourceLocatorProperty);
            set => SetValue(ResourceLocatorProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ResourceLocator"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ResourceLocatorProperty =
            DependencyProperty.Register(nameof(ResourceLocator), typeof(IResourceLocator), typeof(ExtendedStarSizeBehavior));

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

            dataGrid.Loaded += DataGrid_Loaded;
            dataGrid.Unloaded += DataGrid_Unloaded;
        }

        private void DataGrid_Loaded(object sender, EventArgs e)
        {
            var dataGrid = (DataGrid)sender;

            dataGrid.BeginInvoke(DispatcherPriority.Background, () => DataGrid_Loaded(dataGrid));
        }

        private void DataGrid_Loaded(DataGrid dataGrid)
        {
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

        private void DataGrid_Unloaded(object? sender, RoutedEventArgs e)
        {
            var dataGrid = (DataGrid?)sender;
            if (dataGrid == null)
                return;

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

        private void DataGrid_ColumnDisplayIndexChanged(object? sender, DataGridColumnEventArgs e)
        {
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            HijackStarSizeColumnsInfo(dataGrid);
            UpdateColumnWidths(dataGrid, null, (e.Action == NotifyCollectionChangedAction.Add) ? UpdateMode.ResetStarSize : UpdateMode.Default);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void DataGrid_NonFrozenColumnsViewportHorizontalOffsetChanged(object? sender, EventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            if (_changingGridSizeCounter > 0)
                return;

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void ScrollViewer_ViewportWidthChanged(object? sender, EventArgs e)
        {
            _changingGridSizeCounter += 1;

            var dataGrid = AssociatedObject;

            dataGrid?.BeginInvoke(() =>
            {
                UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
                _changingGridSizeCounter -= 1;
            });
        }

        private void DataGrid_ColumnActualWidthChanged(object? sender, DataGridColumnEventArgs e)
        {
            var column = e.Column;
            if (column == null)
                return;

            if (sender is not DataGrid dataGrid)
                return;

            if (_changingGridSizeCounter > 0)
                return;

            if (column.DisplayIndex < dataGrid.FrozenColumnCount)
                return; // wait for NonFrozenColumnsViewportHorizontalOffset change

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, column, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void DataGrid_ColumnVisibilityChanged(object? sender, EventArgs e)
        {
            if (sender is not DataGrid dataGrid)
                return;

            UpdateColumnWidths(dataGrid, null, UpdateMode.ResetStarSize);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void UpdateColumnWidths(DataGrid dataGrid, DataGridColumn? modifiedColumn, UpdateMode updateMode)
        {
            var dataGridColumns = dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .Skip(dataGrid.FrozenColumnCount)
                .Where(c => c.Visibility == Visibility.Visible)
                .ToArray();

            _columnsAreFitWithinViewPort = !ApplyStarSize(dataGridColumns, modifiedColumn) && DistributeAvailableSize(dataGrid, dataGridColumns, modifiedColumn, updateMode);
        }

        private static bool ApplyStarSize(IEnumerable<DataGridColumn> dataGridColumns, DataGridColumn? modifiedColumn)
        {
            if ((modifiedColumn == null) || (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
                return false;

            var starSize = GetStarSize(modifiedColumn);
            if (starSize < double.Epsilon)
                return false;

            var starFactor = modifiedColumn.ActualWidth / starSize;

            foreach (var column in dataGridColumns.SkipWhile(c => !Equals(c, modifiedColumn)).Skip(1))
            {
                starSize = GetStarSize(column);
                if (starSize > double.Epsilon)
                {
                    column.Width = starSize * starFactor;
                }
            }

            return true;
        }

        private bool DistributeAvailableSize(DataGrid dataGrid, DataGridColumn[] dataGridColumns, DataGridColumn? modifiedColumn, UpdateMode updateMode)
        {
            var scrollViewer = _scrollViewer;
            if (scrollViewer == null)
                return false;

            var startColumnIndex = modifiedColumn?.DisplayIndex ?? 0;

            bool IsFixedColumn(DataGridColumn c)
            {
                return (GetStarSize(c) <= double.Epsilon) || (c.DisplayIndex <= startColumnIndex);
            }

            bool IsVariableColumn(DataGridColumn c)
            {
                return !IsFixedColumn(c);
            }

            var fixedColumnsWidth = dataGridColumns
                .Where(IsFixedColumn)
                .Select(c => c.ActualWidth)
                .Sum();

            var getEffectiveColumnSize = (updateMode == UpdateMode.ResetStarSize) ? (Func<DataGridColumn, double>)GetStarSize : GetActualWidth;

            var variableColumnWidths = dataGridColumns
                .Where(IsVariableColumn)
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

            foreach (var column in dataGridColumns.Where(IsVariableColumn))
            {
                column.Width = getEffectiveColumnSize(column) * factor;
            }

            return true;
        }

        private static void HijackStarSizeColumnsInfo(DataGrid dataGrid)
        {
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

            DataGridColumn? leftColumn = null;
            var isLeftColumnStarSized = false;

            foreach (var column in dataGrid.Columns.OrderBy(c => c.DisplayIndex))
            {
                var leftColumnRightGripperToolTip = (ToolTip?)leftColumn?.GetValue(RightGripperToolTipProperty);
                var thisColumnLeftGripperToolTip = (ToolTip?)column.GetValue(LeftGripperToolTipProperty);

                var visibility = isLeftColumnStarSized ? Visibility.Visible : Visibility.Collapsed;

                if (leftColumnRightGripperToolTip != null)
                    leftColumnRightGripperToolTip.Visibility = visibility;

                if (thisColumnLeftGripperToolTip != null)
                    thisColumnLeftGripperToolTip.Visibility = visibility;

                leftColumn = column;
                isLeftColumnStarSized = GetStarSize(column) > double.Epsilon;
            }

            if (leftColumn?.GetValue(RightGripperToolTipProperty) is ToolTip toolTip)
                toolTip.Visibility = Visibility.Collapsed;
        }

        private void InjectColumnHeaderStyle(DataGrid dataGrid)
        {
            var baseStyle = dataGrid.ColumnHeaderStyle ?? (Style)dataGrid.FindResource(typeof(DataGridColumnHeader));

            if (baseStyle.Setters.OfType<Setter>().Any(setter => setter.Property == ColumnHeaderGripperExtenderProperty))
                return;

            var newStyle = new Style(typeof(DataGridColumnHeader), baseStyle);
            newStyle.Setters.Add(new Setter(ColumnHeaderGripperExtenderProperty, this));
            dataGrid.ColumnHeaderStyle = newStyle;
        }

        private static double GetActualWidth(DataGridColumn column)
        {
            return column.ActualWidth;
        }

        private static double GetStarSize(DataGridColumn column)
        {
            return (double)column.GetValue(StarSizeProperty);
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
            if (d is not DataGridColumnHeader columnHeader)
                return;

            if (e.NewValue is not ExtendedStarSizeBehavior self)
                return;

            var dataGrid = self.AssociatedObject;

            dataGrid?.BeginInvoke(DispatcherPriority.Background, () =>
            {
                self.ApplyGripperToolTip(columnHeader, @"PART_LeftHeaderGripper", LeftGripperToolTipProperty);
                self.ApplyGripperToolTip(columnHeader, @"PART_RightHeaderGripper", RightGripperToolTipProperty);
            });
        }

        private void ApplyGripperToolTip(DataGridColumnHeader columnHeader, string gripperName, DependencyProperty toolTipProperty)
        {
            if (columnHeader.Template?.FindName(gripperName, columnHeader) is not Thumb gripper)
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
    }
}
