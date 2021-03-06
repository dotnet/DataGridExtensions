namespace DataGridExtensions
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Defines the attached properties that can be set on the data grid column level.
    /// </summary>
    public static class DataGridFilterColumn
    {
        #region IsFilterVisible attached property

        /// <summary>
        /// Control the visibility of the filter for this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>True if the filter is visible for this column</returns>
        /// <exception cref="ArgumentNullException">column</exception>
        public static bool GetIsFilterVisible(this DataGridColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            return (bool)column.GetValue(IsFilterVisibleProperty);
        }
        /// <summary>
        /// Control the visibility of the filter for this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">column</exception>
        public static void SetIsFilterVisible(this DataGridColumn column, bool value)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            column.SetValue(IsFilterVisibleProperty, value);
        }
        /// <summary>
        /// Identifies the IsFilterVisible dependency property
        /// </summary>
        public static readonly DependencyProperty IsFilterVisibleProperty =
            DependencyProperty.RegisterAttached("IsFilterVisible", typeof(bool), typeof(DataGridFilterColumn), new FrameworkPropertyMetadata(true));

        #endregion

        #region Template attached property

        /// <summary>
        /// Gets the control template for the filter of this column. If the template is null or unset, a default template will be used.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The control template.</returns>
        public static ControlTemplate? GetTemplate(this DataGridColumn column)
        {
            return (ControlTemplate)column.GetValue(TemplateProperty);
        }
        /// <summary>
        /// Sets the control template for the filter of this column. If the template is null or unset, a default template will be used.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public static void SetTemplate(this DataGridColumn column, ControlTemplate? value)
        {
            column.SetValue(TemplateProperty, value);
        }
        /// <summary>
        /// Identifies the Template dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(ControlTemplate), typeof(DataGridFilterColumn));

        #endregion

        #region FilterHost attached property

        /// <summary>
        /// Gets the filter host for the data grid of this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The <see cref="DataGridFilterHost"/></returns>
        public static DataGridFilterHost? GetFilterHost(this DataGridColumn column)
        {
            return (DataGridFilterHost?)column.GetValue(FilterHostProperty);
        }
        /// <summary>
        /// Sets the filter host for the data grid of this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public static void SetFilterHost(this DataGridColumn column, DataGridFilterHost? value)
        {
            column.SetValue(FilterHostProperty, value);
        }
        /// <summary>
        /// Identifies the FilterHost dependency property.
        /// </summary>
        public static readonly DependencyProperty FilterHostProperty =
            DependencyProperty.RegisterAttached("FilterHost", typeof(DataGridFilterHost), typeof(DataGridFilterColumn));

        #endregion

        #region DataGridFilterColumnControl attached property

        /// <summary>
        /// Gets the filter host for the data grid of this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The <see cref="DataGridFilterColumnControl"/></returns>
        public static DataGridFilterColumnControl? GetDataGridFilterColumnControl(this DataGridColumn column)
        {
            return (DataGridFilterColumnControl?)column.GetValue(DataGridFilterColumnControlProperty);
        }
        /// <summary>
        /// Sets the filter host for the data grid of this column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public static void SetDataGridFilterColumnControl(this DataGridColumn column, DataGridFilterColumnControl? value)
        {
            column.SetValue(DataGridFilterColumnControlProperty, value);
        }
        /// <summary>
        /// Identifies the DataGridFilterColumnControl dependency property.
        /// </summary>
        public static readonly DependencyProperty DataGridFilterColumnControlProperty =
            DependencyProperty.RegisterAttached("DataGridFilterColumnControl", typeof(DataGridFilterColumnControl), typeof(DataGridFilterColumn));

        #endregion

        #region Filter attached property

        /// <summary>
        /// Gets the filter expression of the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The filter expression.</returns>
        public static object? GetFilter(this DataGridColumn column)
        {
            return column.GetValue(FilterProperty);
        }
        /// <summary>
        /// Sets the filter expression of the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public static void SetFilter(this DataGridColumn column, object? value)
        {
            column.SetValue(FilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(object), typeof(DataGridFilterColumn), new FrameworkPropertyMetadata(null, Filter_Changed));

        private static void Filter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (!(d is DataGridColumn column))
                return;

            // Update the effective filter. If the filter is provided as content, the content filter will be recreated when needed.
            column.SetActiveFilter(args.NewValue as IContentFilter);
            column.GetFilterHost()?.OnFilterChanged();
        }

        #endregion

        #region ActiveFilter attached property

        /// <summary>
        /// Gets the filter expression of the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The filter.</returns>
        public static IContentFilter? GetActiveFilter(this DataGridColumn column)
        {
            return (IContentFilter)column.GetValue(ActiveFilterProperty);
        }
        /// <summary>
        /// Sets the filter expression of the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <param name="value">The value.</param>
        public static void SetActiveFilter(this DataGridColumn column, IContentFilter? value)
        {
            column.SetValue(ActiveFilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty ActiveFilterProperty =
            DependencyProperty.RegisterAttached("ActiveFilter", typeof(IContentFilter), typeof(DataGridFilterColumn));

        #endregion

        /// <summary>
        /// Creates a new content filter.
        /// </summary>
        internal static IContentFilter CreateContentFilter(this DataGrid dataGrid, object? content)
        {
            return dataGrid.GetContentFilterFactory().Create(content);
        }

        /// <summary>
        /// Returns true if the given item matches the filter condition for this column.
        /// </summary>
        internal static bool Matches(this DataGridColumn column, DataGrid dataGrid, object? item)
        {
            var activeFilter = column.GetActiveFilter();
            if (activeFilter == null)
            {
                var filter = column.GetFilter();
                if (filter == null)
                    return true;

                activeFilter = dataGrid.CreateContentFilter(filter);
                column.SetActiveFilter(activeFilter);
            }

            return activeFilter.IsMatch(GetCellContentData(column, item));
        }

        /// <summary>
        /// Identifies the CellValue dependency property, a private helper property used to evaluate the property path for the list items.
        /// </summary>
        private static readonly DependencyProperty _cellValueProperty =
            DependencyProperty.Register("_cellValue", typeof(object), typeof(DataGridFilterColumn));

        /// <summary>
        /// Examines the property path and returns the objects value for this column.
        /// Filtering is applied on the SortMemberPath, this is the path used to create the binding.
        /// </summary>
        internal static object? GetCellContentData(this DataGridColumn column, object? item)
        {
            var propertyPath = column.SortMemberPath;
            if (item == null ||string.IsNullOrEmpty(propertyPath))
                return null;

            try
            {
                var type = item.GetType();
                var property = type.GetProperty(propertyPath);
                if (property != null)
                {
                    return property.GetValue(item);
                }
            }
            catch
            {
                // not a plain property, fall-back to binding...
            }

            // Since already the name "SortMemberPath" implies that this might be not only a simple property name but a full property path
            // we use binding for evaluation; this will properly handle even complex property paths like e.g. "SubItems[0].Name"
            BindingOperations.SetBinding(column, _cellValueProperty, new Binding(propertyPath) { Source = item });
            var propertyValue = column.GetValue(_cellValueProperty);
            BindingOperations.ClearBinding(column, _cellValueProperty);

            return propertyValue;
        }
    }
}
