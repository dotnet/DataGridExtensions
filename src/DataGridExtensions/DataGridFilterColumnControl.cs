namespace DataGridExtensions
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
    using System.Windows.Media;
    using System.Windows.Threading;

    using DataGridExtensions.Framework;

    using Throttle;

    /// <summary>
    /// This class is the control hosting all information needed for filtering of one column.
    /// Filtering is enabled by simply adding this control to the header template of the DataGridColumn.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DataGridFilterColumnControl : Control, INotifyPropertyChanged
    {
        private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static readonly ControlTemplate _emptyControlTemplate = new ControlTemplate();

        static DataGridFilterColumnControl()
        {
            var templatePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(TemplateProperty, typeof(Control));
            if (templatePropertyDescriptor != null)
                templatePropertyDescriptor.DesignerCoerceValueCallback = Template_CoerceValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DataGridExtensions.DataGridFilterColumnControl" /> class.
        /// </summary>
        public DataGridFilterColumnControl()
        {
            Loaded += Self_Loaded;
            Unloaded += Self_Unloaded;

            Focusable = false;
            DataContext = this;
        }

        private void Self_Loaded(object sender, RoutedEventArgs e)
        {
            // Find the ancestor column header and data grid controls.
            ColumnHeader = this.FindAncestorOrSelf<DataGridColumnHeader>();

            var column = ColumnHeader?.Column;
            if (column == null)
                return;

            if (column.ClipboardContentBinding == null && !string.IsNullOrEmpty(column.SortMemberPath))
            {
                column.ClipboardContentBinding = new Binding(column.SortMemberPath);
            }

            DataGrid = ColumnHeader.FindAncestorOrSelf<DataGrid>() ?? throw new InvalidOperationException("DataGridFilterColumnControl must be a child element of a DataGridColumnHeader.");

            // Find our host and attach our self.
            FilterHost = DataGrid.GetFilter();
            FilterHost.AttachColumnControl(column, this);

            DataGrid.SourceUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.TargetUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.RowEditEnding += DataGrid_RowEditEnding;
            ((INotifyCollectionChanged)DataGrid.Items).CollectionChanged += DataGrid_CollectionChanged;

            // Must set a non-null empty template here, else we won't get the coerce value callback when the columns attached property is null!
            Template = _emptyControlTemplate;

            // Bind our IsFilterVisible and Template properties to the corresponding properties attached to the
            // DataGridColumnHeader.Column property. Use binding instead of simple assignment since columnHeader.Column is still null at this point.
            var isFilterVisiblePropertyPath = new PropertyPath("(0)", DataGridFilterColumn.IsFilterVisibleProperty);
            BindingOperations.SetBinding(this, VisibilityProperty, new Binding() { Path = isFilterVisiblePropertyPath, Source = column, Mode = BindingMode.OneWay, Converter = _booleanToVisibilityConverter });

            var templatePropertyPath = new PropertyPath("(0)", DataGridFilterColumn.TemplateProperty);
            BindingOperations.SetBinding(this, TemplateProperty, new Binding() { Path = templatePropertyPath, Source = column, Mode = BindingMode.OneWay });

            var filterPropertyPath = new PropertyPath("(0)", DataGridFilterColumn.FilterProperty);
            BindingOperations.SetBinding(this, FilterProperty, new Binding() { Path = filterPropertyPath, Source = column, Mode = BindingMode.TwoWay });
        }

        private void Self_Unloaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = DataGrid;
            if (dataGrid != null)
            {
                dataGrid.SourceUpdated -= DataGrid_SourceOrTargetUpdated;
                dataGrid.TargetUpdated -= DataGrid_SourceOrTargetUpdated;
                dataGrid.RowEditEnding -= DataGrid_RowEditEnding;
                ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged -= DataGrid_CollectionChanged;
            }

            // Clear all bindings generated during load.
            BindingOperations.ClearBinding(this, VisibilityProperty);
            BindingOperations.ClearBinding(this, TemplateProperty);
            BindingOperations.ClearBinding(this, FilterProperty);
        }

        private void DataGrid_SourceOrTargetUpdated(object? sender, DataTransferEventArgs e)
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
            {
                ValuesUpdated();
            }
        }

        private void DataGrid_RowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
        {
            ValuesUpdated();
        }

        private void DataGrid_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ValuesUpdated();
        }

        /// <summary>
        /// The user provided filter (IFilter) or content (usually a string) used to filter this column.
        /// If the filter object implements IFilter, it will be used directly as the filter,
        /// else the filter object will be passed to the content filter.
        /// </summary>
        public object? Filter
        {
            get => GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterColumnControl));

        private static object? Template_CoerceValue(DependencyObject sender, object? baseValue)
        {
            if (baseValue != null)
                return baseValue;

            if (!(sender is DataGridFilterColumnControl control))
                return null;

            // Just resolved the binding to the template property attached to the column, and the value has not been set on the column:
            // => try to find the default template based on the columns type.
            var columnType = control.ColumnHeader?.Column?.GetType();
            if (columnType == null)
                return null;

            var resourceKey = new ComponentResourceKey(typeof(DataGridFilter), columnType);

            return control.DataGrid?.GetResourceLocator()?.FindResource(control, resourceKey) ?? control.TryFindResource(resourceKey);
        }

        /// <summary>
        /// Returns all distinct visible (filtered) values of this column as string.
        /// This can be used to e.g. feed the ItemsSource of an AutoCompleteBox to give a hint to the user what to enter.
        /// </summary>
        /// <remarks>
        /// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date
        /// values when the source object changes.
        /// </remarks>
        public IEnumerable<string> Values => InternalValues().Distinct().ToList().AsReadOnly();

        /// <summary>
        /// Returns all distinct source values of this column as string.
        /// This can be used to e.g. feed the ItemsSource of an Excel-like auto-filter that always shows all source values that can be selected. 
        /// </summary>
        /// <remarks>
        /// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date 
        /// values when the source object changes.
        /// </remarks>
        public IEnumerable<string> SourceValues
        {
            get
            {
                // use the global filter, if any...
                var predicate = FilterHost?.CreatePredicate(null) ?? (_ => true);

                return InternalSourceValues(predicate).Distinct().ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Returns all distinct selectable values of this column as string.
        /// This can be used to e.g. feed the ItemsSource of an Excel-like auto-filter, that only shows the values that are currently selectable, depending on the other filters.
        /// </summary>
        /// <remarks>
        /// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date 
        /// values when the source object changes.
        /// </remarks>
        public IEnumerable<string> SelectableValues
        {
            get
            {
                // filter by all columns except this.
                var predicate = FilterHost?.CreatePredicate(FilterHost.GetFilteredColumns(Column)) ?? (_ => true);

                return InternalSourceValues(predicate).Distinct().ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Returns a flag indicating whether this column has some filter condition to evaluate or not.
        /// If there is no filter condition we don't need to invoke this filter.
        /// </summary>
        public bool IsFiltered => !string.IsNullOrWhiteSpace(Filter?.ToString()) && ColumnHeader?.Column != null;

        /// <summary>
        /// Notification of the filter that the content of the values might have changed.
        /// </summary>
        [Throttled(typeof(DispatcherThrottle), (int)DispatcherPriority.Background)]
        internal void ValuesUpdated()
        {
            // We simply raise a change event for the properties and create the output on the fly in the getter of the properties;
            // if there is no binding to the properties we don't waste resources to compute a list that is never used.
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(SourceValues));
            OnPropertyChanged(nameof(SelectableValues));
        }

        /// <summary>
        /// Gets the column this control is hosting the filter for.
        /// </summary>
        public DataGridColumn? Column => ColumnHeader?.Column;

        /// <summary>
        /// The DataGrid we belong to.
        /// </summary>
        protected DataGrid? DataGrid
        {
            get;
            private set;
        }

        /// <summary>
        /// The filter we belong to.
        /// </summary>
        protected DataGridFilterHost? FilterHost
        {
            get;
            private set;
        }

        /// <summary>
        /// The column header of the column we are filtering. This control must be a child element of the column header.
        /// </summary>
        protected DataGridColumnHeader? ColumnHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// The actual filter control generated at runtime. 
        /// </summary>
        public Control? FilterControl
        {
            get;
            set;
        }

        /// <summary>
        /// Populate the FilterControl property once the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (VisualTreeHelper.GetChildrenCount(this) == 1
                && VisualTreeHelper.GetChild(this, 0) is Control filterControl)
            {
                FilterControl = filterControl;
            }
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        protected IEnumerable<string> InternalValues()
        {
            return DataGrid?.Items
                .Cast<object>()
                .Where(item => item != null)
                .Select(item => Column?.GetCellContentData(item))
                .Select(content => content?.ToString() ?? string.Empty) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        protected IEnumerable<string> InternalSourceValues(Predicate<object> predicate)
        {
            var itemsSource = DataGrid?.ItemsSource;

            if (itemsSource == null)
                return Enumerable.Empty<string>();

            var collectionView = itemsSource as ICollectionView;

            var items = collectionView?.SourceCollection ?? itemsSource;

            return items.Cast<object>()
                .Where(item => item != null && predicate(item))
                .Select(item => Column?.GetCellContentData(item))
                .Select(content => content?.ToString() ?? string.Empty);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}