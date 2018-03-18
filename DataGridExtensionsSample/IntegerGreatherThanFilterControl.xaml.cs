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
        private TextBox _textBox;

        public IntegerGreatherThanFilterControl()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _textBox = (TextBox)sender;
            var text = _textBox.Text;
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
            DependencyProperty.Register("Filter", typeof(IContentFilter), typeof(IntegerGreatherThanFilterControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((IntegerGreatherThanFilterControl)o).Filter_Changed(args.NewValue)));

        private void Filter_Changed(object newValue)
        {
            if ((newValue == null) && (_textBox != null))
            {
                _textBox.Text = string.Empty;
            }
        }

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
