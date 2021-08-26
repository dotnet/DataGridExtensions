namespace DataGridExtensions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// A class to manage the column styles of a <see cref="DataGrid"/>
    /// </summary>
    public static class ColumnStyles
    {
        /// <summary>
        /// Gets the default column styles.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <returns>The styles.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static DataGridColumnStyleCollection? GetDefaultColumnStyles(DataGrid dataGrid)
        {
            return (DataGridColumnStyleCollection?)dataGrid.GetValue(DefaultColumnStylesProperty);
        }
        /// <summary>
        /// Sets the default column styles.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">The styles.</param>
        public static void SetDefaultColumnStyles(DataGrid dataGrid, DataGridColumnStyleCollection? value)
        {
            dataGrid.SetValue(DefaultColumnStylesProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:DataGridExtensions.ColumnStyles.DefaultColumnStyles"/> attached property
        /// </summary>
        public static readonly DependencyProperty DefaultColumnStylesProperty =
            DependencyProperty.RegisterAttached("DefaultColumnStyles", typeof(DataGridColumnStyleCollection), typeof(ColumnStyles), new FrameworkPropertyMetadata(null, DefaultColumnStyles_Changed));

        private static void DefaultColumnStyles_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;

            if (e.NewValue is not DataGridColumnStyleCollection styles)
                return;

            foreach (var col in dataGrid.Columns)
            {
                ApplyStyle(styles, col);
            }

            dataGrid.Columns.CollectionChanged += (_, args) => Columns_CollectionChanged(styles, args);
        }

        private static void Columns_CollectionChanged(DataGridColumnStyleCollection styles, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Add)
                return;

            if (args.NewItems?[0] is not DataGridColumn column)
                return;

            ApplyStyle(styles, column);
        }

        private static void ApplyStyle(DataGridColumnStyleCollection styles, DependencyObject column)
        {
            var columnType = column.GetType();

            var style = styles.FirstOrDefault(s => s.ColumnType == columnType);
            if (style == null)
                return;

            SetStyleBinding(column, DataGridColumnStyle.ElementStyleProperty, style);
            SetStyleBinding(column, DataGridColumnStyle.EditingElementStyleProperty, style);
        }

        private static void SetStyleBinding(DependencyObject column, DependencyProperty property, DataGridColumnStyle style)
        {
            // ElementStyle and EditingElementStyle are not defined at the base class, but are different property for e.g. bound and combo box column
            // => use reflection to get the effective property for the specified column.

            var propertyName = property.Name;
            var columnType = column.GetType();

            var defaultStyle = columnType.GetProperty("Default" + propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null, null) as Style;
            var activeStyle = columnType.GetProperty(propertyName)?.GetValue(column, null) as Style;

            if (activeStyle != defaultStyle)
                return;

            if (columnType.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null) is DependencyProperty targetProperty)
            {
                BindingOperations.SetBinding(column, targetProperty, new Binding(propertyName) { Source = style, FallbackValue = defaultStyle });
            }
        }
    }

    /// <summary>
    /// A collection of <see cref="DataGridColumnStyle"/> objects.
    /// </summary>
    public class DataGridColumnStyleCollection : Collection<DataGridColumnStyle>
    {

    }

    /// <summary>
    /// Defines the column styles for a data grid column.
    /// </summary>
    public class DataGridColumnStyle : DependencyObject
    {
        /// <summary>
        /// Gets or sets the type of the column for which to set the styles.
        /// </summary>
        public Type? ColumnType { get; set; }

        /// <summary>
        /// Gets or sets the element style for the column.
        /// </summary>
        public Style? ElementStyle
        {
            get => (Style)GetValue(ElementStyleProperty);
            set => SetValue(ElementStyleProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ElementStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ElementStyleProperty =
            DependencyProperty.Register("ElementStyle", typeof(Style), typeof(DataGridColumnStyle));

        /// <summary>
        /// Gets or sets the editing element style for the column.
        /// </summary>
        public Style? EditingElementStyle
        {
            get => (Style)GetValue(EditingElementStyleProperty);
            set => SetValue(EditingElementStyleProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="EditingElementStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty EditingElementStyleProperty =
            DependencyProperty.Register("EditingElementStyle", typeof(Style), typeof(DataGridColumnStyle));
    }
}
