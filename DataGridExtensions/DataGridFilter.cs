namespace DataGridExtensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Controls;

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
        public static bool GetIsAutoFilterEnabled(this DataGrid obj)
        {
            Contract.Requires(obj != null);

            return obj.GetValue<bool>(IsAutoFilterEnabledProperty);
        }
        /// <summary>
        /// Sets if the default filters are automatically attached to each column. Set to false if you want to control filters by code.
        /// </summary>
        public static void SetIsAutoFilterEnabled(this DataGrid obj, bool value)
        {
            Contract.Requires(obj != null);

            obj.SetValue(IsAutoFilterEnabledProperty, value);
        }
        /// <summary>
        /// Identifies the IsAutoFilterEnabled dependency property
        /// </summary>
        public static readonly DependencyProperty IsAutoFilterEnabledProperty =
            DependencyProperty.RegisterAttached("IsAutoFilterEnabled", typeof(bool), typeof(DataGridFilter), new FrameworkPropertyMetadata(false, IsAutoFilterEnabled_Changed));

        private static void IsAutoFilterEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
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

        public static DataGridFilterHost GetFilter(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);
            Contract.Ensures(Contract.Result<DataGridFilterHost>() != null);

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
        private static readonly DependencyProperty FilterProperty =
            DependencyProperty.RegisterAttached("Filter", typeof(DataGridFilterHost), typeof(DataGridFilter));

        #endregion

        #region ContentFilterFactory attached property

        private static readonly IContentFilterFactory DefaultContentFilterFactory = new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the content filter factory for the data grid filter.
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IContentFilterFactory GetContentFilterFactory(this DataGrid dataGrid)
        {
            Contract.Requires(dataGrid != null);
            Contract.Ensures(Contract.Result<IContentFilterFactory>() != null);

            return (IContentFilterFactory)dataGrid.GetValue(ContentFilterFactoryProperty) ?? DefaultContentFilterFactory;
        }
        /// <summary>
        /// Sets the content filter factory for the data grid filter.
        /// </summary>
        public static void SetContentFilterFactory(this DataGrid dataGrid, IContentFilterFactory value)
        {
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");

            dataGrid.SetValue(ContentFilterFactoryProperty, value);
        }
        /// <summary>
        /// Identifies the ContentFilterFactory dependency property
        /// </summary>
        public static readonly DependencyProperty ContentFilterFactoryProperty =
            DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), new FrameworkPropertyMetadata(DefaultContentFilterFactory, null, ContentFilterFactory_CoerceValue));

        private static object ContentFilterFactory_CoerceValue(DependencyObject sender, object value)
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
        public static TimeSpan GetFilterEvaluationDelay(this DataGrid obj)
        {
            Contract.Requires(obj != null);

            return obj.GetValue<TimeSpan>(FilterEvaluationDelayProperty);
        }

        /// <summary>
        /// Sets the delay that is used to throttle filter changes before the filter is applied.
        /// </summary>
        /// <param name="obj">The data grid</param>
        /// <param name="value">The new throttle delay.</param>
        public static void SetFilterEvaluationDelay(this DataGrid obj, TimeSpan value)
        {
            Contract.Requires(obj != null);

            obj.SetValue(FilterEvaluationDelayProperty, value);
        }
        /// <summary>
        /// Identifies the FilterEvaluationDelay dependency property
        /// </summary>
        public static readonly DependencyProperty FilterEvaluationDelayProperty =
            DependencyProperty.RegisterAttached("FilterEvaluationDelay", typeof(TimeSpan), typeof(DataGridFilter), new FrameworkPropertyMetadata(TimeSpan.FromSeconds(0.5)));

        #endregion

        /// <summary>
        /// Gets the resource locator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The locator</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static IResourceLocator GetResourceLocator(this DataGrid obj)
        {
            Contract.Requires(obj != null);
            return (IResourceLocator)obj.GetValue(ResourceLocatorProperty);
        }
        /// <summary>
        /// Sets the resource locator.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetResourceLocator(this DataGrid obj, IResourceLocator value)
        {
            Contract.Requires(obj != null);
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
        public static readonly DependencyProperty ResourceLocatorProperty =
            DependencyProperty.RegisterAttached("ResourceLocator", typeof(IResourceLocator), typeof(DataGridFilter), new FrameworkPropertyMetadata(null));


        /// <summary>
        /// Gets the value of the <see cref="P:DataGridExtensions.GlobalFilter"/> attached property from a given <see cref="DataGrid"/>.
        /// </summary>
        /// <param name="obj">The <see cref="DataGrid"/> from which to read the property value.</param>
        /// <returns>the value of the <see cref="P:DataGridExtensions.GlobalFilter"/> attached property.</returns>
        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static Predicate<object> GetGlobalFilter(DataGrid obj)
        {
            Contract.Requires(obj != null);

            return (Predicate<object>)obj.GetValue(GlobalFilterProperty);
        }
        /// <summary>
        /// Sets the value of the <see cref="P:DataGridExtensions.GlobalFilter" /> attached property to a given <see cref="DataGrid" />.
        /// </summary>
        /// <param name="obj">The <see cref="DataGrid" /> on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        public static void SetGlobalFilter(DataGrid obj, Predicate<object> value)
        {
            Contract.Requires(obj != null);

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
        public static readonly DependencyProperty GlobalFilterProperty =
            DependencyProperty.RegisterAttached("GlobalFilter", typeof(Predicate<object>), typeof(DataGridFilter), new FrameworkPropertyMetadata(GlobalFilter_Changed));

        private static void GlobalFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Contract.Requires(d != null);

            ((DataGrid)d).GetFilter().SetGlobalFilter((Predicate<object>)e.NewValue);
        }

        #region Resource keys

        /// <summary>
        /// Template for the filter on a column represented by a DataGridTextColumn.
        /// </summary>
        public static readonly ResourceKey TextColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));

        /// <summary>
        /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
        /// </summary>
        public static readonly ResourceKey CheckBoxColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridCheckBoxColumn));

        /// <summary>
        /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
        /// </summary>
        public static readonly ResourceKey TemplateColumnFilterTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTemplateColumn));

        /// <summary>
        /// Template for the whole column header.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderTemplate");

        /// <summary>
        /// The filter icon template.
        /// </summary>
        public static readonly ResourceKey IconTemplateKey = new ComponentResourceKey(typeof(DataGridFilter), "IconTemplate");

        /// <summary>
        /// The filter icon style.
        /// </summary>
        public static readonly ResourceKey IconStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "IconStyle");

        /// <summary>
        /// Style for the filter check box in a filtered DataGridCheckBoxColumn.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderSearchCheckBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchCheckBoxStyle");

        /// <summary>
        /// Style for the filter text box in a filtered DataGridTextColumn.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderSearchTextBoxStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxStyle");

        /// <summary>
        /// Style for the clear button in the filter text box in a filtered DataGridTextColumn.
        /// </summary>
        public static readonly ResourceKey ColumnHeaderSearchTextBoxClearButtonStyleKey = new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxClearButtonStyle");

        #endregion
    }
}
