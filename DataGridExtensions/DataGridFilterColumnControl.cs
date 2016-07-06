namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// This class is the control hosting all information needed for filtering of one column.
    /// Filtering is enabled by simply adding this control to the header template of the DataGridColumn.
    /// </summary>
    public class DataGridFilterColumnControl : Control, INotifyPropertyChanged
    {
        private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static readonly ControlTemplate _emptyControlTemplate = new ControlTemplate();

        /// <summary>
        /// The active filter for this column.
        /// </summary>
        private IContentFilter _activeFilter;

        static DataGridFilterColumnControl()
        {
            var templatePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(TemplateProperty, typeof(Control));

            if (templatePropertyDescriptor != null)
                templatePropertyDescriptor.DesignerCoerceValueCallback = Template_CoerceValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFilterColumnControl"/> class.
        /// </summary>
        public DataGridFilterColumnControl()
        {
            Loaded += self_Loaded;
            Unloaded += self_Unloaded;

            Focusable = false;
            DataContext = this;
        }

        private void self_Loaded(object sender, RoutedEventArgs e)
        {
            if (FilterHost == null)
            {
                // Find the ancestor column header and data grid controls.
                ColumnHeader = this.FindAncestorOrSelf<DataGridColumnHeader>();
                if (ColumnHeader == null)
                    throw new InvalidOperationException("DataGridFilterColumnControl must be a child element of a DataGridColumnHeader.");

                DataGrid = ColumnHeader.FindAncestorOrSelf<DataGrid>();
                if (DataGrid == null)
                    throw new InvalidOperationException("DataGridColumnHeader must be a child element of a DataGrid");

                // Find our host and attach ourself.
                FilterHost = DataGrid.GetFilter();
            }

            FilterHost.AddColumn(this);

            DataGrid.SourceUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.TargetUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.RowEditEnding += DataGrid_RowEditEnding;

            // Must set a non-null empty template here, else we won't get the coerce value callback when the columns attached property is null!
            Template = _emptyControlTemplate;

            // Bind our IsFilterVisible and Template properties to the corresponding properties attached to the
            // DataGridColumnHeader.Column property. Use binding instead of simple assignment since columnHeader.Column is still null at this point.
            var isFilterVisiblePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.IsFilterVisibleProperty);
            BindingOperations.SetBinding(this, VisibilityProperty, new Binding() { Path = isFilterVisiblePropertyPath, Source = ColumnHeader, Mode = BindingMode.OneWay, Converter = _booleanToVisibilityConverter });

            var templatePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.TemplateProperty);
            BindingOperations.SetBinding(this, TemplateProperty, new Binding() { Path = templatePropertyPath, Source = ColumnHeader, Mode = BindingMode.OneWay });

            var filterPropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.FilterProperty);
            BindingOperations.SetBinding(this, FilterProperty, new Binding() { Path = filterPropertyPath, Source = ColumnHeader, Mode = BindingMode.TwoWay });
        }

        private void self_Unloaded(object sender, RoutedEventArgs e)
        {
            // Detach from host.
            // Must check for null, unloaded event might be raised even if no loaded event has been raised before!
            FilterHost?.RemoveColumn(this);

            if (DataGrid != null)
            {
                DataGrid.SourceUpdated -= DataGrid_SourceOrTargetUpdated;
                DataGrid.TargetUpdated -= DataGrid_SourceOrTargetUpdated;
                DataGrid.RowEditEnding -= DataGrid_RowEditEnding;
            }

            // Clear all bindings generated during load.
            BindingOperations.ClearBinding(this, VisibilityProperty);
            BindingOperations.ClearBinding(this, TemplateProperty);
            BindingOperations.ClearBinding(this, FilterProperty);
        }

        private void DataGrid_SourceOrTargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
            {
                ValuesUpdated();
            }
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            this.BeginInvoke(DispatcherPriority.Background, ValuesUpdated);
        }

        /// <summary>
        /// The user provided filter (IFilter) or content (usually a string) used to filter this column.
        /// If the filter object implements IFilter, it will be used directly as the filter,
        /// else the filter object will be passed to the content filter.
        /// </summary>
        public object Filter
        {
            get { return GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterColumnControl), new FrameworkPropertyMetadata(null, (sender, e) => ((DataGridFilterColumnControl)sender).Filter_Changed(e.NewValue)));

        private void Filter_Changed(object newValue)
        {
            // Update the effective filter. If the filter is provided as content, the content filter will be recreated when needed.
            _activeFilter = newValue as IContentFilter;

            // Notify the filter to update the view.
            FilterHost?.OnFilterChanged();
        }

        private static object Template_CoerceValue(DependencyObject sender, object baseValue)
        {
            if (baseValue != null)
                return baseValue;

            var control = sender as DataGridFilterColumnControl;

            // Just resolved the binding to the template property attached to the column, and the value has not been set on the column:
            // => try to find the default template based on the columns type.
            var columnType = control?.ColumnHeader?.Column?.GetType();
            if (columnType == null)
                return null;

            var resourceKey = new ComponentResourceKey(typeof(DataGridFilter), columnType);

            return control.DataGrid.GetResourceLocator()?.FindResource(control, resourceKey) ?? control.TryFindResource(resourceKey);
        }

        /// <summary>
        /// Returns all distinct visible (filtered) values of this column as string.
        /// This can be used to e.g. feed the ItemsSource of an AutoCompleteBox to give a hint to the user what to enter.
        /// </summary>
        public IEnumerable<string> Values
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

                return InternalValues().Distinct().ToArray();
            }
        }

        /// <summary>
        /// Returns all distinct source values of this column as string.
        /// This can be used to e.g. feed the ItemsSource of an Excel-like autofilter.
        /// </summary>
        public IEnumerable<string> SourceValues
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

                return InternalSourceValues().Distinct().ToArray();
            }
        }

        /// <summary>
        /// Returns a flag indicating whether this column has some filter condition to evaluate or not.
        /// If there is no filter condition we don't need to invoke this filter.
        /// </summary>
        public bool IsFiltered => !string.IsNullOrWhiteSpace(Filter?.ToString()) && ColumnHeader?.Column != null;

        /// <summary>
        /// Returns true if the given item matches the filter condition for this column.
        /// </summary>
        internal bool Matches(object item)
        {
            if ((Filter == null) || (FilterHost == null))
                return true;

            if (_activeFilter == null)
            {
                _activeFilter = FilterHost.CreateContentFilter(Filter);
            }

            return _activeFilter.IsMatch(GetCellContent(item));
        }

        /// <summary>
        /// Notification of the filter that the content of the values might have changed.
        /// </summary>
        internal void ValuesUpdated()
        {
            // We simply raise a change event for the properties and create the output on the fly in the getter of the properties;
            // if there is no binding to the properties we don't waste resources to compute a list that is never used.
            OnPropertyChanged(nameof(Values));
            OnPropertyChanged(nameof(SourceValues));
        }

        /// <summary>
        /// Gets the column this control is hosting the filter for.
        /// </summary>
        public DataGridColumn Column => ColumnHeader?.Column;

        /// <summary>
        /// The DataGrid we belong to.
        /// </summary>
        protected DataGrid DataGrid
        {
            get;
            private set;
        }

        /// <summary>
        /// The filter we belong to.
        /// </summary>
        protected DataGridFilterHost FilterHost
        {
            get;
            private set;
        }

        /// <summary>
        /// The column header of the column we are filtering. This control must be a child element of the column header.
        /// </summary>
        protected DataGridColumnHeader ColumnHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifies the CellValue dependency property, a private helper property used to evaluate the property path for the list items.
        /// </summary>
        private static readonly DependencyProperty _cellValueProperty =
            DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterColumnControl));

        /// <summary>
        /// Examines the property path and returns the objects value for this column.
        /// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
        /// </summary>
        protected object GetCellContent(object item)
        {
            var propertyPath = ColumnHeader?.Column.SortMemberPath;

            if (string.IsNullOrEmpty(propertyPath))
                return null;

            // Since already the name "SortMemberPath" implies that this might be not only a simple property name but a full property path
            // we use binding for evaluation; this will properly handle even complex property paths like e.g. "SubItems[0].Name"
            BindingOperations.SetBinding(this, _cellValueProperty, new Binding(propertyPath) { Source = item });
            var propertyValue = GetValue(_cellValueProperty);
            BindingOperations.ClearBinding(this, _cellValueProperty);

            return propertyValue;
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        protected IEnumerable<string> InternalValues()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            return DataGrid?.Items
                .Cast<object>()
                .Select(GetCellContent)
                .Where(content => content != null)
                .Select(content => content.ToString()) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        protected IEnumerable<string> InternalSourceValues()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            var itemsSource = DataGrid?.ItemsSource;

            if (itemsSource == null)
                return Enumerable.Empty<string>();

            var collectionView = itemsSource as ICollectionView;

            var items = collectionView?.SourceCollection ?? itemsSource;

            return items.Cast<object>()
                .Select(GetCellContent)
                .Where(content => content != null)
                .Select(content => content.ToString());
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant((FilterHost == null) || (DataGrid != null));
        }

    }
}