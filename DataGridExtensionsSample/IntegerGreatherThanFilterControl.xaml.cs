namespace DataGridExtensionsSample
{
    using System.Windows;
    using System.Windows.Controls;
    using DataGridExtensions;

    /// <summary>
    /// Interaction logic for IntegerGreatherThanFilterControl.xaml
    /// </summary>
    public partial class IntegerGreatherThanFilterControl
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

            Filter = !int.TryParse(text, out threshold) ? null : new ContentFilter(threshold);
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
            readonly int _threshold;

            public ContentFilter(int threshold)
            {
                _threshold = threshold;
            }

            public bool IsMatch(object value)
            {
                if (value == null)
                    return false;

                int i;

                return int.TryParse(value.ToString(), out i) && (i > _threshold);
            }
        }
    }
}
