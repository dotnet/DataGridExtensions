namespace DataGridExtensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Defines the attached properties that can be set on the data grid column level.
    /// </summary>
    public static class DataGridFilterColumn
    {
        #region IsFilterVisible attached property

        /// <summary>
        /// Control the visibility of the filter for this column.
        /// </summary>
        public static bool GetIsFilterVisible(this DataGridColumn column)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            return column.GetValue<bool>(IsFilterVisibleProperty);
        }
        /// <summary>
        /// Control the visibility of the filter for this column.
        /// </summary>
        public static void SetIsFilterVisible(this DataGridColumn column, bool value)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            column.SetValue(IsFilterVisibleProperty, value);
        }
        /// <summary>
        /// Identifies the IsFilterVisible dependency property
        /// </summary>
        public static readonly DependencyProperty IsFilterVisibleProperty =
            DependencyProperty.RegisterAttached("IsFilterVisible", typeof(bool), typeof(DataGridFilterColumn), new UIPropertyMetadata(true));

        #endregion

        #region Template attached property

        /// <summary>
        /// Gets the control template for the filter of this column. If the template is null or unset, a default template will be used.
        /// </summary>
        public static ControlTemplate GetTemplate(this DataGridColumn column)
        {
            Contract.Requires(column != null);

            return (ControlTemplate)column.GetValue(TemplateProperty);
        }
        /// <summary>
        /// Sets the control template for the filter of this column. If the template is null or unset, a default template will be used.
        /// </summary>
        public static void SetTemplate(this DataGridColumn column, ControlTemplate value)
        {
            Contract.Requires(column != null);

            column.SetValue(TemplateProperty, value);
        }
        /// <summary>
        /// Identifies the Template dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(ControlTemplate), typeof(DataGridFilterColumn));

        #endregion

        #region Filter attached property

        /// <summary>
        /// Gets the filter expression of the column.
        /// </summary>
        public static object GetFilter(this DataGridColumn column)
        {
            Contract.Requires(column != null);

            return column.GetValue(FilterProperty);
        }
        /// <summary>
        /// Sets the filter expression of the column.
        /// </summary>
        public static void SetFilter(this DataGridColumn column, object value)
        {
            Contract.Requires(column != null);

            column.SetValue(FilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(object), typeof(DataGridFilterColumn));

        #endregion
    }
}
