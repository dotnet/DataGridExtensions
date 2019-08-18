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
    using Microsoft.Xaml.Behaviors;

    using System.Windows.Threading;

    using DataGridExtensions.Framework;

    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// Extended start size column behavior. Allows columns to get larger than the client area, but not smaller.
    /// The Resizing behavior can be modified using Ctrl or Shift keys: Ctrl resizes all columns to the right proportionally, Shift fits all columns to the right into the client area.
    /// A tool tip can be attached to the column headers resizing grippers to help the user with these features.
    /// </summary>
    public class ExtendedStarSizeBehavior : Behavior<DataGrid>
    {
        // ReSharper disable AssignNullToNotNullAttribute
        [NotNull]
        private static readonly DependencyPropertyDescriptor ViewportWidthPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(ScrollViewer.ViewportWidthProperty, typeof(ScrollViewer));
        [NotNull]
        private static readonly DependencyPropertyDescriptor NonFrozenColumnsViewportHorizontalOffsetPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(DataGrid.NonFrozenColumnsViewportHorizontalOffsetProperty, typeof(DataGrid));
        // ReSharper restore AssignNullToNotNullAttribute

        [NotNull]
        private readonly DispatcherThrottle _updateColumnGripperToolTipVisibilityThrottle;

        [CanBeNull]
        private ScrollViewer _scrollViewer;

        private int _changingGridSizeCounter;
        private bool _columnsAreFitWithinViewPort;

        /// <summary>
        /// The resource key for the default column header gripper tool tip style.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ColumnHeaderGripperToolTipStyleKey = new ComponentResourceKey(typeof(ExtendedStarSizeBehavior), "ColumnHeaderGripperToolTipStyle");

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DataGridExtensions.Behaviors.ExtendedStarSizeBehavior" /> class.
        /// </summary>
        public ExtendedStarSizeBehavior()
        {
            _updateColumnGripperToolTipVisibilityThrottle = new DispatcherThrottle(DispatcherPriority.Background, UpdateColumnGripperToolTipVisibility);
        }

        /// <summary>
        /// Gets or sets the style of the tool tip for the grippers in the column headers.
        /// </summary>
        [CanBeNull]
        public Style ColumnHeaderGripperToolTipStyle
        {
            get => (Style)GetValue(ColumnHeaderGripperToolTipStyleProperty);
            set => SetValue(ColumnHeaderGripperToolTipStyleProperty, value);
        }
        /// <summary>
        /// Identifies the ColumnHeaderGripperToolTipStyle dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty ColumnHeaderGripperToolTipStyleProperty =
            DependencyProperty.Register("ColumnHeaderGripperToolTipStyle", typeof(Style), typeof(ExtendedStarSizeBehavior));

        /// <summary>
        /// Gets or sets the resource locator.
        /// </summary>
        [CanBeNull]
        public IResourceLocator ResourceLocator
        {
            get => (IResourceLocator)GetValue(ResourceLocatorProperty);
            set => SetValue(ResourceLocatorProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ResourceLocator"/> dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty ResourceLocatorProperty =
            DependencyProperty.Register("ResourceLocator", typeof(IResourceLocator), typeof(ExtendedStarSizeBehavior));

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            var dataGrid = AssociatedObject;

            dataGrid.Loaded += DataGrid_Loaded;
            dataGrid.Unloaded += DataGrid_Unloaded;
        }

        private void DataGrid_Loaded([NotNull] object sender, [NotNull] EventArgs e)
        {
            var dataGrid = (DataGrid)sender;

            dataGrid.BeginInvoke(DispatcherPriority.Background, () => DataGrid_Loaded(dataGrid));
        }

        private void DataGrid_Loaded([NotNull] DataGrid dataGrid)
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

        private void DataGrid_Unloaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
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

        private void DataGrid_ColumnDisplayIndexChanged([NotNull] object sender, [NotNull] DataGridColumnEventArgs e)
        {
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void Columns_CollectionChanged([NotNull] object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            var dataGrid = AssociatedObject;
            if (dataGrid == null)
                return;

            HijackStarSizeColumnsInfo(dataGrid);
            UpdateColumnWidths(dataGrid, null, (e.Action == NotifyCollectionChangedAction.Add) ? UpdateMode.ResetStarSize : UpdateMode.Default);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void DataGrid_NonFrozenColumnsViewportHorizontalOffsetChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            var dataGrid = (DataGrid)sender;

            if (_changingGridSizeCounter > 0)
                return;

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void ScrollViewer_ViewportWidthChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            _changingGridSizeCounter += 1;

            var dataGrid = AssociatedObject;

            dataGrid?.BeginInvoke(() =>
            {
                UpdateColumnWidths(dataGrid, null, UpdateMode.Default);
                _changingGridSizeCounter -= 1;
            });
        }

        private void DataGrid_ColumnActualWidthChanged([NotNull] object sender, [NotNull] DataGridColumnEventArgs e)
        {
            var colum = e.Column;
            if (colum == null)
                return;

            var dataGrid = (DataGrid)sender;

            if (_changingGridSizeCounter > 0)
                return;

            if (colum.DisplayIndex < dataGrid.FrozenColumnCount)
                return; // wait for NonFrozenColumnsViewportHorizontalOffset change

            _changingGridSizeCounter += 1;
            UpdateColumnWidths(dataGrid, colum, UpdateMode.Default);
            _changingGridSizeCounter -= 1;
        }

        private void DataGrid_ColumnVisibilityChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            UpdateColumnWidths((DataGrid)sender, null, UpdateMode.ResetStarSize);
            _updateColumnGripperToolTipVisibilityThrottle.Tick();
        }

        private void UpdateColumnWidths([NotNull] DataGrid dataGrid, [CanBeNull] DataGridColumn modifiedColum, UpdateMode updateMode)
        {
            // ReSharper disable PossibleNullReferenceException
            var dataGridColumns = dataGrid.Columns
                .OrderBy(c => c.DisplayIndex)
                .Skip(dataGrid.FrozenColumnCount)
                .Where(c => (c.Visibility == Visibility.Visible))
                .ToArray();
            // ReSharper restore PossibleNullReferenceException

            _columnsAreFitWithinViewPort = !ApplyStarSize(dataGridColumns, modifiedColum) && DistributeAvailableSize(dataGrid, dataGridColumns, modifiedColum, updateMode);
        }

        private static bool ApplyStarSize([NotNull, ItemNotNull] IEnumerable<DataGridColumn> dataGridColumns, [CanBeNull] DataGridColumn modifiedColum)
        {
            if ((modifiedColum == null) || (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
                return false;

            var starSize = GetStarSize(modifiedColum);
            if (starSize < double.Epsilon)
                return false;

            var starFactor = modifiedColum.ActualWidth / starSize;

            foreach (var column in dataGridColumns.SkipWhile(c => !Equals(c, modifiedColum)).Skip(1))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                starSize = GetStarSize(column);
                if (starSize > double.Epsilon)
                {
                    column.Width = starSize * starFactor;
                }
            }

            return true;
        }

        private bool DistributeAvailableSize([NotNull] DataGrid dataGrid, [NotNull, ItemNotNull] DataGridColumn[] dataGridColumns, [CanBeNull] DataGridColumn modifiedColum, UpdateMode updateMode)
        {
            var scrollViewer = _scrollViewer;
            if (scrollViewer == null)
                return false;

            var startColumnIndex = modifiedColum?.DisplayIndex ?? 0;

            // ReSharper disable AssignNullToNotNullAttribute
            bool IsFixedColumn(DataGridColumn c) => (GetStarSize(c) <= double.Epsilon) || (c.DisplayIndex <= startColumnIndex);
            bool IsVariableColumn(DataGridColumn c) => !IsFixedColumn(c);
            // ReSharper restore AssignNullToNotNullAttribute

            var fixedColumnsWidth = dataGridColumns
                .Where(IsFixedColumn)
                // ReSharper disable once PossibleNullReferenceException
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

        private static void HijackStarSizeColumnsInfo([NotNull] DataGrid dataGrid)
        {
            foreach (var column in dataGrid.Columns)
            {
                // ReSharper disable once PossibleNullReferenceException
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

            // ReSharper disable once PossibleNullReferenceException
            foreach (var column in dataGrid.Columns.OrderBy(c => c.DisplayIndex))
            {
                var leftColumnRightGripperToolTip = (ToolTip)leftColumn?.GetValue(RightGripperToolTipProperty);
                var thisColumnLeftGripperToolTip = (ToolTip)column.GetValue(LeftGripperToolTipProperty);

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

        private void InjectColumnHeaderStyle([NotNull] DataGrid dataGrid)
        {
            var baseStyle = dataGrid.ColumnHeaderStyle ?? (Style)dataGrid.FindResource(typeof(DataGridColumnHeader));

            if (baseStyle.Setters.OfType<Setter>().Any(setter => setter.Property == ColumnHeaderGripperExtenderProperty))
                return;

            var newStyle = new Style(typeof(DataGridColumnHeader), baseStyle);
            newStyle.Setters.Add(new Setter(ColumnHeaderGripperExtenderProperty, this));
            dataGrid.ColumnHeaderStyle = newStyle;
        }

        private static double GetActualWidth([NotNull] DataGridColumn column)
        {
            return column.ActualWidth;
        }

        private static double GetStarSize([NotNull] DataGridColumn column)
        {
            return column.GetValue<double>(StarSizeProperty);
        }

        [NotNull]
        private static readonly DependencyProperty StarSizeProperty =
            DependencyProperty.RegisterAttached("StarSize", typeof(double), typeof(ExtendedStarSizeBehavior), new FrameworkPropertyMetadata(0.0));

        [NotNull]
        private static readonly DependencyProperty LeftGripperToolTipProperty =
            DependencyProperty.RegisterAttached("LeftGripperToolTip", typeof(ToolTip), typeof(ExtendedStarSizeBehavior));

        [NotNull]
        private static readonly DependencyProperty RightGripperToolTipProperty =
            DependencyProperty.RegisterAttached("RightGripperToolTip", typeof(ToolTip), typeof(ExtendedStarSizeBehavior));

        [NotNull]
        private static readonly DependencyProperty ColumnHeaderGripperExtenderProperty =
            DependencyProperty.RegisterAttached("ColumnHeaderGripperExtender", typeof(ExtendedStarSizeBehavior), typeof(ExtendedStarSizeBehavior), new FrameworkPropertyMetadata(null, ColumnHeaderGripperExtender_Changed));

        private static void ColumnHeaderGripperExtender_Changed([NotNull] DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DataGridColumnHeader columnHeader))
                return;

            if (!(e.NewValue is ExtendedStarSizeBehavior self))
                return;

            var dataGrid = self.AssociatedObject;

            dataGrid?.BeginInvoke(DispatcherPriority.Background, () =>
            {
                self.ApplyGripperToolTip(columnHeader, @"PART_LeftHeaderGripper", LeftGripperToolTipProperty);
                self.ApplyGripperToolTip(columnHeader, @"PART_RightHeaderGripper", RightGripperToolTipProperty);
            });
        }

        private void ApplyGripperToolTip([NotNull] DataGridColumnHeader columnHeader, [NotNull] string gripperName, [NotNull] DependencyProperty toolTipProperty)
        {
            if (!(columnHeader.Template?.FindName(gripperName, columnHeader) is Thumb gripper))
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
