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

            return (bool)obj.GetValue(IsAutoFilterEnabledProperty);
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
            if (dataGrid != null)
            {
                // Force creation of the host and show or hide the controls.
                dataGrid.GetFilter().Enable(true.Equals(e.NewValue));
            }
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

            return (IContentFilterFactory)dataGrid.GetValue(ContentFilterFactoryProperty);
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
            DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), new UIPropertyMetadata(DefaultContentFilterFactory, null, ContentFilterFactory_CoerceValue));

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

            return (TimeSpan) obj.GetValue(FilterEvaluationDelayProperty);
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
            DependencyProperty.RegisterAttached("FilterEvaluationDelay", typeof(TimeSpan), typeof(DataGridFilter), new UIPropertyMetadata(TimeSpan.FromSeconds(0.5)));

        #endregion

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
            ((DataGrid) d).GetFilter().SetGlobalFilter((Predicate<object>) e.NewValue);
        }

        #region Resource keys

        /// <summary>
        /// Template for the filter on a column represented by a DataGridTextColumn.
        /// </summary>
        public static ResourceKey TextColumnFilterTemplateKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));
            }
        }

        /// <summary>
        /// Template for the filter on a column represented by a DataGridCheckBoxColumn.
        /// </summary>
        public static ResourceKey CheckBoxColumnFilterTemplateKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridCheckBoxColumn));
            }
        }

        /// <summary>
        /// Template for the whole column header.
        /// </summary>
        public static ResourceKey ColumnHeaderTemplateKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridColumn));
            }
        }

        /// <summary>
        /// The filter icon template.
        /// </summary>
        public static ResourceKey IconTemplateKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), "IconTemplate");
            }
        }

        /// <summary>
        /// The filter icon style.
        /// </summary>
        public static ResourceKey IconStyleKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), "IconStyle");
            }
        }

        /// <summary>
        /// Style for the filter check box in a filtered DataGridCheckBoxColumn.
        /// </summary>
        public static ResourceKey ColumnHeaderSearchCheckBoxStyleKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchCheckBoxStyle");
            }
        }

        /// <summary>
        /// Style for the filter text box in a filtered DataGridTextColumn.
        /// </summary>
        public static ResourceKey ColumnHeaderSearchTextBoxStyleKey
        {
            get
            {
                Contract.Ensures(Contract.Result<ResourceKey>() != null);

                return new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxStyle");
            }
        }

        #endregion
    }
}
