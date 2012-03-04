using System;
using System.Windows;
using System.Windows.Controls;

namespace DataGridExtensions
{
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

            return (bool)column.GetValue(IsFilterVisibleProperty);
        }
        /// <summary>
        /// Control the visibility of the filter for this column.
        /// </summary>
        public static void SetIsFilterVisible(this DataGridColumn column, bool value)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            column.SetValue(DataGridFilterColumn.IsFilterVisibleProperty, value);
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
            if (column == null)
                throw new ArgumentNullException("column");

            return (ControlTemplate)column.GetValue(TemplateProperty);
        }
        /// <summary>
        /// Sets the control template for the filter of this column. If the template is null or unset, a default template will be used.
        /// </summary>
        public static void SetTemplate(this DataGridColumn column, ControlTemplate value)
        {
            if (column == null)
                throw new ArgumentNullException("column");

            column.SetValue(TemplateProperty, value);
        }
        /// <summary>
        /// Identifies the Template dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.RegisterAttached("Template", typeof(ControlTemplate), typeof(DataGridFilterColumn));

        #endregion
    }
}
