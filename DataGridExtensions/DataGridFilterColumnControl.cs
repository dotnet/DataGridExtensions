using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace DataGridExtensions
{
    /// <summary>
    /// This class is the control hosting all information needed for filtering of one column.
    /// Filtering is enabled by simply adding this control to the header template of the DataGridColumn.
    /// </summary>
    public sealed class DataGridFilterColumnControl : Control, INotifyPropertyChanged
    {
        private static BooleanToVisibilityConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private static ControlTemplate EmptyControlTemplate = new ControlTemplate();

        /// <summary>
        /// The column header of the column we are filtering. This control must be a child element of the column header.
        /// </summary>
        private DataGridColumnHeader columnHeader;
        /// <summary>
        /// The filter we belong to.
        /// </summary>
        private DataGridFilterHost filterHost;
        /// <summary>
        /// The active filter for this column.
        /// </summary>
        private IContentFilter activeFilter;

        static DataGridFilterColumnControl()
        {
            var templatePropertyDescriptor = DependencyPropertyDescriptor.FromProperty(TemplateProperty, typeof(Control));
            templatePropertyDescriptor.DesignerCoerceValueCallback = Template_CoerceValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFilterColumnControl"/> class.
        /// </summary>
        public DataGridFilterColumnControl()
        {
            Loaded += self_Loaded;
            Unloaded += self_Loaded;

            Focusable = false;
            this.DataContext = this;
        }

        void self_Loaded(object sender, RoutedEventArgs e)
        {
            if (filterHost == null)
            {
                // Find the ancestor column header and data grid controls.
                columnHeader = this.FindAncestorOrSelf<DataGridColumnHeader>();
                if (columnHeader == null)
                    throw new InvalidOperationException("DataGridFilterColumnControl must be a child element of a DataGridColumnHeader.");

                var dataGrid = columnHeader.FindAncestorOrSelf<DataGrid>();
                if (dataGrid == null)
                    throw new InvalidOperationException("DataGridColumnHeader must be a child element of a DataGrid");

                // Find our host and attach oursef.
                filterHost = dataGrid.GetFilter();
                filterHost.AddColumn(this);
            }

            // Must set a non-null empty template here, else we won't get the coerce value callback when the columns attached property is null!
            Template = EmptyControlTemplate;

            // Bind our IsFilterVisible and Template properties to the corresponding properties attached to the
            // DataGridColumnHeader.Column property. Use binding instead of simple assignment since columnHeader.Column is still null at this point.
            var isFilterVisiblePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.IsFilterVisibleProperty);
            BindingOperations.SetBinding(this, VisibilityProperty, new Binding() { Path = isFilterVisiblePropertyPath, Source = columnHeader, Mode = BindingMode.OneWay, Converter = BooleanToVisibilityConverter });

            var templatePropertyPath = new PropertyPath("Column.(0)", DataGridFilterColumn.TemplateProperty);
            BindingOperations.SetBinding(this, TemplateProperty, new Binding() { Path = templatePropertyPath, Source = columnHeader, Mode = BindingMode.OneWay });
        }

        void self_Unloaded(object sender, RoutedEventArgs e)
        {
            // Detach from host.
            filterHost.RemoveColumn(this);
            // Clear all bindings generatend during load. 
            BindingOperations.ClearBinding(this, VisibilityProperty);
            BindingOperations.ClearBinding(this, TemplateProperty);
        }

        #region Filter dependency property

        /// <summary>
        /// The user provided filter (IFilter) or content (usually a string) used to filter this column. 
        /// If the filter object implements IFilter, it will be used directly as the filter,
        /// else the filter object will be passed to the content filter.
        /// </summary>
        public object Filter
        {
            get { return (object)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(object), typeof(DataGridFilterColumnControl), new UIPropertyMetadata(null, new PropertyChangedCallback((sender, e) => ((DataGridFilterColumnControl)sender).Filter_Changed(e.NewValue))));

        private void Filter_Changed(object newValue)
        {
            // Update the effective filter. If the filter is provided as content, the content filter will be recreated when needed.
            activeFilter = newValue as IContentFilter;
            // Notify the filter to update the view.
            filterHost.FilterChanged();
        }

        #endregion

        private static object Template_CoerceValue(DependencyObject sender, object baseValue)
        {
            if (baseValue == null)
            {
                var control = sender as DataGridFilterColumnControl;
                if (control != null)
                {
                    // Just resolved the binding to the template property attached to the column, and the value has not been set on the column:
                    // => try to find the default template based on the columns type.
                    var column = control.columnHeader.Column;
                    if (column != null)
                    {
                        return control.TryFindResource(new ComponentResourceKey(typeof(DataGridFilter), column.GetType())) as ControlTemplate;
                    }
                }
            }

            return baseValue;
        }

        /// <summary>
        /// Returns all distinct visible (filtered) values of this column as string. 
        /// This can be used to e.g. feed the ItemsSource of an AutoCompleteBox to give a hint to the user what to enter.
        /// </summary>
        public IEnumerable<string> Values
        {
            get
            {
                return InternalValues().Distinct();
            }
        }

        /// <summary>
        /// Returns a flag indicating whether this column has some filter condition to evaluate or not. 
        /// If there is no filter condition we don't need to invoke this filter.
        /// </summary>
        internal bool IsFiltered
        {
            get
            {
                return (Filter != null) && !string.IsNullOrWhiteSpace(Filter.ToString()) && (this.columnHeader.Column != null);
            }
        }

        /// <summary>
        /// Returns true if the given item matches the filter condition for this column.
        /// </summary>
        internal bool Matches(object item)
        {
            if (Filter == null)
                return true;

            if (activeFilter == null)
            {
                activeFilter = filterHost.CreateContentFilter(Filter);
            }

            var propertyValue = GetCellContent(item);

            if (propertyValue == null)
                return false;

            return activeFilter.IsMatch(propertyValue);
        }

        /// <summary>
        /// Notification of the filter that the content of the values might have changed. 
        /// </summary>
        internal void ValuesUpdated()
        {
            // We simply raise a change event for the Values propety and create the output on the fly in the getter of the Values property;
            // if there is no binding to the values property we don't waste resources to compute a list that is never used.
            OnPropertyChanged("Values");
        }

        internal DataGridColumn Column
        {
            get
            {
                return columnHeader.Column;
            }
        }

        /// <summary>
        /// Identifies the CellValue dependency property, a private helper property used to evaluate the property path for the list items.
        /// </summary>
        private static readonly DependencyProperty CellValueProperty =
            DependencyProperty.Register("CellValue", typeof(object), typeof(DataGridFilterColumnControl));

        /// <summary>
        /// Examines the property path and returns the objects value for this column. 
        /// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
        /// </summary>
        private object GetCellContent(object item)
        {
            var column = columnHeader.Column;
            if (column == null)
                return null;

            var propertyPath = column.SortMemberPath;

            // Since already the name "SortMemberPath" implies that this might be not only a simple property name but a full property path
            // we use binding for evaluation; this will properly handle even complex property paths like e.g. "SubItems[0].Name"
            BindingOperations.SetBinding(this, CellValueProperty, new Binding(propertyPath) { Source = item });
            var propertyValue = GetValue(CellValueProperty);
            BindingOperations.ClearBinding(this, CellValueProperty);

            return propertyValue;
        }

        /// <summary>
        /// Gets the cell content of all list items for this column.
        /// </summary>
        private IEnumerable<string> InternalValues()
        {
            var dataGrid = columnHeader.FindAncestorOrSelf<DataGrid>();
            var items = dataGrid.Items;

            foreach (var item in items)
            {
                var propertyValue = GetCellContent(item);

                if (propertyValue != null)
                {
                    yield return propertyValue.ToString();
                }
            }
        }

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string propertyName)
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
    }
}