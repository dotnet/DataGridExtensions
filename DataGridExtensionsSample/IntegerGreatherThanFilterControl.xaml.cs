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
using System.Globalization;

namespace DataGridExtensionsSample
{
    /// <summary>
    /// Interaction logic for IntegerGreatherThanFilterControl.xaml
    /// </summary>
    public partial class IntegerGreatherThanFilterControl : Control
    {
        public IntegerGreatherThanFilterControl()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            var text = textBox.Text;
            int threshold;

            this.Filter = !int.TryParse(text, out threshold) ? null : new ContentFilter(threshold);
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
            DependencyProperty.Register("Filter", typeof(IContentFilter), typeof(IntegerGreatherThanFilterControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        class ContentFilter : IContentFilter
        {
            readonly int threshold;

            public ContentFilter(int threshold)
            {
                this.threshold = threshold;
            }

            public bool IsMatch(object value)
            {
                if (value == null)
                    return false;

                int i;

                return int.TryParse(value.ToString(), out i) && (i > this.threshold);
            }
        }
    }
}
