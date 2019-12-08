namespace DataGridExtensions
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using JetBrains.Annotations;

    /// <summary>
    /// Defines the attached properties that can be set on the data grid level.
    /// </summary>
    public static class DataGridFilter
    {
        #region IsAutoFilterEnabled attached property

        /// <summary>
        /// Gets if the default filters are automatically attached to each column.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetIsAutoFilterEnabled([NotNull] this DataGrid obj)
        {
            return obj.GetValue<bool>(IsAutoFilterEnabledProperty);
        }
        /// <summary>
        /// Sets if the default filters are automatically attached to each column. Set to false if you want to control filters by code.
        /// </summary>
        public static void SetIsAutoFilterEnabled([NotNull] this DataGrid obj, bool value)
        {
            obj.SetValue(IsAutoFilterEnabledProperty, value);
        }
        /// <summary>
        /// Identifies the IsAutoFilterEnabled dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty IsAutoFilterEnabledProperty =
            DependencyProperty.RegisterAttached("IsAutoFilterEnabled", typeof(bool), typeof(DataGridFilter), new FrameworkPropertyMetadata(false, IsAutoFilterEnabled_Changed));

        private static void IsAutoFilterEnabled_Changed([NotNull] DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            // Force creation of the host and show or hide the controls.
            dataGrid?.GetFilter().Enable(true.Equals(e.NewValue));
        }

        #endregion

        #region Filter attached property

        /// <summary>
        /// Filter attached property to attach the DataGridFilterHost instance to the owning DataGrid.
        /// This property is only used by code and is not accessible from XAML.
        /// </summary>
        [NotNull]
        public static DataGridFilterHost GetFilter([NotNull] this DataGrid dataGrid)
        {
            var value = (DataGridFilterHost)dataGrid.GetValue(FilterProperty);
            if (value == null)
            {
                value = new DataGridFilterHost(dataGrid);
                dataGrid.SetValue(FilterProperty, value);
            }
            return value;
        }
        /// <summary>
        /// Identifies the Filters dependency property.
        /// This property definition is private, so it's only accessible by code and can't be messed up by invalid bindings.
        /// </summary>
        [NotNull]
        private static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterHost), typeof(DataGridFilter));

        #endregion

        #region ContentFilterFactory attached property

        [NotNull]
        private static readonly IContentFilterFactory DefaultContentFilterFactory = new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the content filter factory for the data grid filter.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        [NotNull]
        public static IContentFilterFactory GetContentFilterFactory([NotNull] this DataGrid dataGrid)
        {
            return (IContentFilterFactory)dataGrid.GetValue(ContentFilterFactoryProperty) ?? DefaultContentFilterFactory;
        }
        /// <summary>
        /// Sets the content filter factory for the data grid filter.
        /// </summary>
        public static void SetContentFilterFactory([NotNull] this DataGrid dataGrid, [CanBeNull] IContentFilterFactory value)
        {
            if (dataGrid == null)
                throw new ArgumentNullException(nameof(dataGrid));

            dataGrid.SetValue(ContentFilterFactoryProperty, value);
        }
        /// <summary>
        /// Identifies the ContentFilterFactory dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty ContentFilterFactoryProperty =
            DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), new FrameworkPropertyMetadata(DefaultContentFilterFactory, null, ContentFilterFactory_CoerceValue));

        [NotNull]
        private static object ContentFilterFactory_CoerceValue([NotNull] DependencyObject sender, [CanBeNull] object value)
        {
            // Ensure non-null content filter.
            return value ?? DefaultContentFilterFactory;
        }

        #endregion

        #region Delay of the filter evaluation throttle.

        /// <summary>
        /// Gets the delay that is used to throttle filter changes before the filter is applied.
        /// </summary>
        /// <param name="obj">The data grid</param>
        /// <returns>The throttle delay.</returns>
        public static TimeSpan GetFilterEvaluationDelay([NotNull] this DataGrid obj)
        {
            return obj.GetValue<TimeSpan>(FilterEvaluationDelayProperty);
        }

        /// <summary>
        /// Sets the delay that is used to throttle filter changes before the filter is applied.
        /// </summary>
        /// <param name="obj">The data grid</param>
        /// <param name="value">The new throttle delay.</param>
        public static void SetFilterEvaluationDelay([NotNull] this DataGrid obj, TimeSpan value)
        {
            obj.SetValue(FilterEvaluationDelayProperty, value);
        }
        /// <summary>
        /// Identifies the FilterEvaluationDelay dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty FilterEvaluationDelayProperty =
            DependencyProperty.RegisterAttached("FilterEvaluationDelay", typeof(TimeSpan), typeof(DataGridFilter), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(0.5)));

        #endregion

        /// <summary>
        /// Gets the resource locator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The locator</returns>
        [CanBeNull]
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IResourceLocator GetResourceLocator([NotNull] this DataGrid obj)
        {
            return (IResourceLocator)obj.GetValue(ResourceLocatorProperty);
        }
        /// <summary>
        /// Sets the resource locator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetResourceLocator([NotNull] this DataGrid obj, [CanBeNull] IResourceLocator value)
        {
            obj.SetValue(ResourceLocatorProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:DataGridExtensions.DataGridFilter.ResourceLocator"/> attached property
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Set an resource locator to locate resource if the component resource keys can not be found, e.g. because dgx is used in a plugin and multiple assemblies with resources might exist.
        /// </summary>
        /// </AttachedPropertyComments>
        [NotNull]
        public static readonly DependencyProperty ResourceLocatorProperty =
            DependencyProperty.RegisterAttached("ResourceLocator", typeof(IResourceLocator), typeof(DataGridFilter), new FrameworkPropertyMetadata(null));


        /// <summary>
        /// Gets the value of the <see cref="P:DataGridExtensions.GlobalFilter"/> attached property from a given <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="obj">The <see cref="DataGrid"/> from which to read the property value.</param>
        /// <returns>the value of the <see cref="P:DataGridExtensions.GlobalFilter"/> attached property.</returns>
        [CanBeNull]
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static Predicate<object> GetGlobalFilter([NotNull] DataGrid obj)
        {
            return (Predicate<object>)obj.GetValue(GlobalFilterProperty);
        }
        /// <summary>
        /// Sets the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property to a given <see cref="DataGrid" />.
        /// </summary>
        /// <param name="obj">The <see cref="DataGrid" /> on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetGlobalFilter([NotNull] DataGrid obj, [CanBeNull] Predicate<object> value)
        {
            obj.SetValue(GlobalFilterProperty, value);
        }
        /// <summary>
        /// Identifies the <see cref="P:DataGridExtensions.GlobalFilter"/> dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Allows to specify a global filter that is applied to the items in addition to the column filters.
        /// </summary>
        /// </AttachedPropertyComments>
        [NotNull]
        public static readonly DependencyProperty GlobalFilterProperty =
            DependencyProperty.RegisterAttached("GlobalFilter", typeof(Predicate<object>), typeof(DataGridFilter), new FrameworkPropertyMetadata(GlobalFilter_Changed));

        private static void GlobalFilter_Changed([NotNull] DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataGrid)d).GetFilter().SetGlobalFilter((Predicate<object>)e.NewValue);
        }

        #region Resource keys

        /// <summary>
        /// Template for the filter on a column represented by a DataGridTextColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey TextColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));

        /// <summary>
        /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey CheckBoxColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridCheckBoxColumn));

        /// <summary>
        /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey TemplateColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTemplateColumn));

        /// <summary>
        /// Template for the filter on a column represented by a DataGridComboBoxColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ComboBoxColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridComboBoxColumn));

        /// <summary>
        /// Template for the whole column header.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ColumnHeaderTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderTemplate");

        /// <summary>
        /// The filter icon template.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey IconTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "IconTemplate");

        /// <summary>
        /// The filter icon style.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey IconStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "IconStyle");

        /// <summary>
        /// Style for the filter check box in a filtered DataGridCheckBoxColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ColumnHeaderSearchCheckBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchCheckBoxStyle");

        /// <summary>
        /// Style for the filter text box in a filtered DataGridTextColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ColumnHeaderSearchTextBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxStyle");

        /// <summary>
        /// Style for the clear button in the filter text box in a filtered DataGridTextColumn.
        /// </summary>
        [NotNull]
        public static readonly ResourceKey ColumnHeaderSearchTextBoxClearButtonStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxClearButtonStyle");

        #endregion
    }
}
