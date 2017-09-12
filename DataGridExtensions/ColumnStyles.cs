namespace DataGridExtensions
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using JetBrains.Annotations;

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
        [CanBeNull, ItemNotNull]
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static DataGridColumnStyleCollection GetDefaultColumnStyles([NotNull] DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);
            return (DataGridColumnStyleCollection)dataGrid.GetValue(DefaultColumnStylesProperty);
        }
        /// <summary>
        /// Sets the default column styles.
        /// </summary>
        /// <param name="dataGrid">The data grid.</param>
        /// <param name="value">The styles.</param>
        public static void SetDefaultColumnStyles([NotNull] DataGrid dataGrid, [CanBeNull, ItemNotNull] DataGridColumnStyleCollection value)
        {
            Contract.Requires(dataGrid != null);
            dataGrid.SetValue(DefaultColumnStylesProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:DataGridExtensions.ColumnStyles.DefaultColumnStyles"/> attached property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty DefaultColumnStylesProperty =
            DependencyProperty.RegisterAttached("DefaultColumnStyles", typeof(DataGridColumnStyleCollection), typeof(ColumnStyles), new FrameworkPropertyMetadata(null, DefaultColumnStyles_Changed));

        [ContractVerification(false)]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private static void DefaultColumnStyles_Changed(DependencyObject d, [NotNull] DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;

            var styles = (DataGridColumnStyleCollection)e.NewValue;
            if (styles == null)
                return;

            dataGrid.Columns.ForEach(col => ApplyStyle(styles, col));
            dataGrid.Columns.CollectionChanged += (_, args) => Columns_CollectionChanged(styles, args);
        }

        [ContractVerification(false)]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void Columns_CollectionChanged([NotNull, ItemNotNull] DataGridColumnStyleCollection styles, [NotNull] NotifyCollectionChangedEventArgs args)
        {
            if (args.Action != NotifyCollectionChangedAction.Add)
                return;

            var column = (DataGridColumn)args.NewItems[0];

            ApplyStyle(styles, column);
        }

        private static void ApplyStyle([NotNull, ItemNotNull] DataGridColumnStyleCollection styles, [NotNull] DependencyObject column)
        {
            Contract.Requires(styles != null);
            Contract.Requires(column != null);

            var style = styles.FirstOrDefault(s => s.ColumnType == column.GetType());

            if (style == null)
                return;

            SetStyleBinding(column, DataGridColumnStyle.ElementStyleProperty, style);
            SetStyleBinding(column, DataGridColumnStyle.EditingElementStyleProperty, style);
        }

        private static void SetStyleBinding([NotNull] DependencyObject column, [NotNull] DependencyProperty property, [NotNull] DataGridColumnStyle style)
        {
            Contract.Requires(column != null);
            Contract.Requires(property != null);
            Contract.Requires(style != null);

            // ElementStyle and EditingElementStyle are not defined at the base class, but are different property for e.g. bound and combo box column
            // => use reflection to get the effective property for the specified column.

            var propertyName = property.Name;
            Contract.Assume(propertyName != null);
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
        [CanBeNull]
        public Type ColumnType { get; set; }

        /// <summary>
        /// Gets or sets the element style for the column.
        /// </summary>
        [CanBeNull]
        public Style ElementStyle
        {
            get => (Style)GetValue(ElementStyleProperty);
            set => SetValue(ElementStyleProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="ElementStyle"/> dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty ElementStyleProperty =
            DependencyProperty.Register("ElementStyle", typeof(Style), typeof(DataGridColumnStyle));

        /// <summary>
        /// Gets or sets the editing element style for the column.
        /// </summary>
        [CanBeNull]
        public Style EditingElementStyle
        {
            get => (Style)GetValue(EditingElementStyleProperty);
            set => SetValue(EditingElementStyleProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="EditingElementStyle"/> dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty EditingElementStyleProperty =
            DependencyProperty.Register("EditingElementStyle", typeof(Style), typeof(DataGridColumnStyle));
    }
}
