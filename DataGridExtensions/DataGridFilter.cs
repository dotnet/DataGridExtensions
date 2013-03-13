using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DataGridExtensions
{
    /// <summary>
    /// Defines the attached properties that can be set on the data grid level.
    /// </summary>
    public static class DataGridFilter
    {
        #region IsAutoFilterEnabled attached property

        /// <summary>
        /// Gets if the default filters are atomatically attached to each column.
        /// </summary>
        public static bool GetIsAutoFilterEnabled(this DataGrid obj)
        {
            return (bool)obj.GetValue(IsAutoFilterEnabledProperty);
        }
        /// <summary>
        /// Sets if the default filters are atomatically attached to each column. Set to false if you want to control filters by code.
        /// </summary>
        public static void SetIsAutoFilterEnabled(this DataGrid obj, bool value)
        {
            obj.SetValue(IsAutoFilterEnabledProperty, value);
        }
        /// <summary>
        /// Identifies the IsAutoFilterEnabled dependency property
        /// </summary>
        public static readonly DependencyProperty IsAutoFilterEnabledProperty =
            DependencyProperty.RegisterAttached("IsAutoFilterEnabled", typeof(bool), typeof(DataGridFilter), new UIPropertyMetadata(false, IsAutoFilterEnabled_Changed));

        private static void IsAutoFilterEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                // Force creation of the host.
                dataGrid.GetFilter();
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
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");

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

        private static readonly IContentFilterFactory defaultContentFilterFactory = new SimpleContentFilterFactory(StringComparison.CurrentCultureIgnoreCase);

        /// <summary>
        /// Gets the content filter factory for the data grid filter.
        /// </summary>
        public static IContentFilterFactory GetContentFilterFactory(this DataGrid dataGrid)
        {
            if (dataGrid == null)
                throw new ArgumentNullException("dataGrid");
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
            DependencyProperty.RegisterAttached("ContentFilterFactory", typeof(IContentFilterFactory), typeof(DataGridFilter), new UIPropertyMetadata(defaultContentFilterFactory, null, ContentFilterFactory_CoerceValue));

        private static object ContentFilterFactory_CoerceValue(DependencyObject sender, object value)
        {
            // Ensure non-null content filter.
            return value ?? defaultContentFilterFactory;
        }

        #endregion

        #region Resource keys

        /// <summary>
        /// Template for the filter on a colum represented by a DataGridTextColumn. 
        /// </summary>
        public static ResourceKey TextColumnFilterTemplateKey
        {
            get
            {
                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridTextColumn));
            }
        }

        /// <summary>
        /// Template for the filter on a colum represented by a DataGridCheckBoxColumn.
        /// </summary>
        public static ResourceKey CheckBoxColumnFilterTemplateKey
        {
            get
            {
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
                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridColumn));
            }
        }

        /// <summary>
        /// Default style applied to the column header.
        /// </summary>
        public static ResourceKey DataGridColumnHeaderDefaultStyleKey
        {
            get
            {
                return new ComponentResourceKey(typeof(DataGridFilter), typeof(DataGridColumnHeader));
            }
        }

        /// <summary>
        /// The filter icon template.
        /// </summary>
        public static ResourceKey IconTemplateKey
        {
            get
            {
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
                return new ComponentResourceKey(typeof(DataGridFilter), "ColumnHeaderSearchTextBoxStyle");
            }
        }

        #endregion
    }
}
