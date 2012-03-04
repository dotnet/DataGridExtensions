using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataGridExtensions;

namespace DataGridExtensionsSample
{
    /// <summary>
    /// Interaction logic for FilterWithPopupControl.xaml
    /// </summary>
    public partial class FilterWithPopupControl : Control
    {
        public FilterWithPopupControl()
        {
            InitializeComponent();
        }


        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        /// <summary>
        /// Identifies the Minimum dependency property
        /// </summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(FilterWithPopupControl)
                , new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                    , new PropertyChangedCallback((sender, e) => ((FilterWithPopupControl)sender).Range_Changed())));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        /// <summary>
        /// Identifies the Maximum dependency property
        /// </summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(FilterWithPopupControl)
                , new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                    , new PropertyChangedCallback((sender, e) => ((FilterWithPopupControl)sender).Range_Changed())));

        
        private void Range_Changed()
        {
            if (Maximum > Minimum)
            {
                Filter = new ContentFilter(Minimum, Maximum);
            }
            else
            {
                Filter = null;
            }
        }

        
        public IContentFilter Filter
        {
            get { return (IContentFilter)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(IContentFilter), typeof(FilterWithPopupControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        class ContentFilter : IContentFilter
        {
            private double min, max;

            public ContentFilter(double min, double max)
            {
                this.min = min;
                this.max = max;
            }

            public bool IsMatch(object value)
            {
                double number;

                if (!double.TryParse(value.ToString(), out number))
                {
                    return false;
                }

                return (number >= min) && (number <= max);
            }
        }

    }
}
