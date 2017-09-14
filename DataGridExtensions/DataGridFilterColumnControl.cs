using System.Diagnostics;

namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Threading;

    using JetBrains.Annotations;

    /// <summary>
    /// This class is the control hosting all information needed for filtering of one column.
    /// Filtering is enabled by simply adding this control to the header template of the DataGridColumn.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DataGridFilterColumnControl : Control, INotifyPropertyChanged
    {
        [NotNull]
        private static readonly BooleanToVisibilityConverter _booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        [NotNull]
        private static readonly ControlTemplate _emptyControlTemplate = new ControlTemplate();

        /// <summary>
        /// The active filter for this column.
        /// </summary>
        [CanBeNull]
        private IContentFilter _activeFilter;

        static DataGridFilterColumnControl()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var templatePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(TemplateProperty, typeof(Control));

            if (templatePropertyDescriptor != null)
                templatePropertyDescriptor.DesignerCoerceValueCallback = Template_CoerceValue;
        }

        /// <inheritdoc />
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

        private void Self_Loaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (FilterHost == null)
            {
                // Find the ancestor column header and data grid controls.
                ColumnHeader = this.FindAncestorOrSelf<DataGridColumnHeader>();

                DataGrid = ColumnHeader?.FindAncestorOrSelf<DataGrid>() ?? throw new InvalidOperationException("DataGridFilterColumnControl must be a child element of a DataGridColumnHeader.");

                // Find our host and attach ourself.
                FilterHost = DataGrid.GetFilter();
            }

            FilterHost.AddColumn(this);

            // ReSharper disable PossibleNullReferenceException
            DataGrid.SourceUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.TargetUpdated += DataGrid_SourceOrTargetUpdated;
            DataGrid.RowEditEnding += DataGrid_RowEditEnding;
            // ReSharper restore PossibleNullReferenceException

            // Must set a non-null empty template here, else we won't get the coerce value callback when the columns attached property is null!
            Template = _emptyControlTemplate;

            // Bind our IsFilterVisible and Template properties to the corresponding properties attached to the
            // DataGridColumnHeader.Column property. Use binding instead of simple assignment since columnHeader.Column is still null at this point.
            var isFilterVisiblePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.IsFilterVisibleProperty);
            // ReSharper disable once AssignNullToNotNullAttribute
            BindingOperations.SetBinding(this, VisibilityProperty, new Binding() { Path = isFilterVisiblePropertyPath, Source = ColumnHeader, Mode = BindingMode.OneWay, Converter = _booleanToVisibilityConverter });

            var templatePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.TemplateProperty);
            // ReSharper disable once AssignNullToNotNullAttribute
            BindingOperations.SetBinding(this, TemplateProperty, new Binding() { Path = templatePropertyPath, Source = ColumnHeader, Mode = BindingMode.OneWay });

            var filterPropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.FilterProperty);
            BindingOperations.SetBinding(this, FilterProperty, new Binding() { Path = filterPropertyPath, Source = ColumnHeader, Mode = BindingMode.TwoWay });
        }

        private void Self_Unloaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            // Detach from host.
            // Must check for null, unloaded event might be raised even if no loaded event has been raised before!
            FilterHost?.RemoveColumn(this);

            var dataGrid = DataGrid;
            if (dataGrid != null)
            {
                dataGrid.SourceUpdated -= DataGrid_SourceOrTargetUpdated;
                dataGrid.TargetUpdated -= DataGrid_SourceOrTargetUpdated;
                dataGrid.RowEditEnding -= DataGrid_RowEditEnding;
            }

            // Clear all bindings generated during load.
            // ReSharper disable once AssignNullToNotNullAttribute
            BindingOperations.ClearBinding(this, VisibilityProperty);
            // ReSharper disable once AssignNullToNotNullAttribute
            BindingOperations.ClearBinding(this, TemplateProperty);
            BindingOperations.ClearBinding(this, FilterProperty);
        }

        private void DataGrid_SourceOrTargetUpdated([NotNull] object sender, [NotNull] DataTransferEventArgs e)
        {
            if (e.Property == ItemsControl.ItemsSourceProperty)
            {
                ValuesUpdated();
            }
        }

        private void DataGrid_RowEditEnding([NotNull] object sender, [NotNull] DataGridRowEditEndingEventArgs e)
        {
            this.BeginInvoke(DispatcherPriority.Background, ValuesUpdated);
        }

        /// <summary>
        /// The user provided filter (IFilter) or content (usually a string) used to filter this column.
        /// If the filter object implements IFilter, it will be used directly as the filter,
        /// else the filter object will be passed to the content filter.
        /// </summary>
        [CanBeNull]
        public object Filter
        {
            get => GetValue(FilterProperty); 
            set => SetValue(FilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty FilterProperty =
            // ReSharper disable once PossibleNullReferenceException
            DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterColumnControl), new FrameworkPropertyMetadata(null, (sender, e) => ((DataGridFilterColumnControl)sender).Filter_Changed(e.NewValue)));

        private void Filter_Changed([CanBeNull] object newValue)
        {
            // Update the effective filter. If the filter is provided as content, the content filter will be recreated when needed.
            _activeFilter = newValue as IContentFilter;

            // Notify the filter to update the view.
            FilterHost?.OnFilterChanged();
        }

        [CanBeNull]
        private static object Template_CoerceValue([NotNull] DependencyObject sender, [CanBeNull] object baseValue)
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
        [NotNull, ItemNotNull]
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
        /// This can be used to e.g. feed the ItemsSource of an Excel-like auto-filter that always shows all source values that can be selected. 
        /// </summary>
        /// <remarks>
        /// You may need to include "NotifyOnTargetUpdated=true" in the binding of the DataGrid.ItemsSource to get up-to-date 
        /// values when the source object changes.
        /// </remarks>
        [NotNull, ItemNotNull]
        public IEnumerable<string> SourceValues
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

                // use the global filter, if any...
                var predicate = FilterHost?.CreatePredicate(null) ?? (_ => true);

                return InternalSourceValues(predicate)
                    .Distinct()
                    .ToArray();
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
        [NotNull, ItemNotNull]
        public IEnumerable<string> SelectableValues
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

                // filter by all columns except this.
                var predicate = FilterHost?.CreatePredicate(FilterHost.GetColumnFilters(this)) ?? (_ => true);

                return InternalSourceValues(predicate)
                    .Distinct()
                    .ToArray();
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
        internal bool Matches([CanBeNull] object item)
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
            OnPropertyChanged(nameof(SelectableValues));
        }

        /// <summary>
        /// Gets the column this control is hosting the filter for.
        /// </summary>
        [CanBeNull]
        public DataGridColumn Column => ColumnHeader?.Column;

        /// <summary>
        /// The DataGrid we belong to.
        /// </summary>
        [CanBeNull]
        protected DataGrid DataGrid
        {
            get;
            private set;
        }

        /// <summary>
        /// The filter we belong to.
        /// </summary>
        [CanBeNull]
        protected DataGridFilterHost FilterHost
        {
            get;
            private set;
        }

        /// <summary>
        /// The column header of the column we are filtering. This control must be a child element of the column header.
        /// </summary>
        [CanBeNull]
        protected DataGridColumnHeader ColumnHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Identifies the CellValue dependency property, a private helper property used to evaluate the property path for the list items.
        /// </summary>
        [NotNull]
        private static readonly DependencyProperty _cellValueProperty =
            DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterColumnControl));

        /// <summary>
        /// Examines the property path and returns the objects value for this column.
        /// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
        /// </summary>
        [CanBeNull]
        protected object GetCellContent([CanBeNull] object item)
        {
            // ReSharper disable once PossibleNullReferenceException
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
        [NotNull, ItemNotNull]
        protected IEnumerable<string> InternalValues()
        {
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            return DataGrid?.Items
                .Cast<object>()
                .Select(GetCellContent)
                .Select(content => content?.ToString() ?? string.Empty) ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        [NotNull, ItemNotNull]
        protected IEnumerable<string> InternalSourceValues([NotNull] Predicate<object> predicate)
        {
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

            var itemsSource = DataGrid?.ItemsSource;

            if (itemsSource == null)
                return Enumerable.Empty<string>();

            var collectionView = itemsSource as ICollectionView;

            var items = collectionView?.SourceCollection ?? itemsSource;

            return items.Cast<object>()
                .Where(item => predicate(item))
                .Select(GetCellContent)
                .Select(content => content?.ToString() ?? string.Empty);
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged([NotNull] string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        [Conditional("CONTRACTS_FULL")]
        private void ObjectInvariant()
        {
            Contract.Invariant((FilterHost == null) || (DataGrid != null));
        }

    }
}