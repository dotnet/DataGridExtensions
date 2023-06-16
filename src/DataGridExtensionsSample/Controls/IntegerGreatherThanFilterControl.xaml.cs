namespace DataGridExtensionsSample.Controls
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;

    using DataGridExtensions;

    /// <summary>
    /// Interaction logic for IntegerGreatherThanFilterControl.xaml
    /// </summary>
    public partial class IntegerGreatherThanFilterControl
    {
        private TextBox? _textBox;

        public IntegerGreatherThanFilterControl()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = Template.FindName("textBox", this) as TextBox;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var text = ((TextBox)sender).Text;

            Filter = !int.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var threshold) ? null : new ContentFilter(threshold);
        }

        public IContentFilter? Filter
        {
            get => (IContentFilter?)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(nameof(Filter), typeof(IContentFilter), typeof(IntegerGreatherThanFilterControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (o, args) => ((IntegerGreatherThanFilterControl)o).Filter_Changed(args.NewValue)));

        private void Filter_Changed(object newValue)
        {
            var textBox = _textBox;
            if (textBox == null)
                return;

            textBox.Text = (newValue as ContentFilter)?.Value ?? string.Empty;
        }

        private sealed class ContentFilter : IContentFilter
        {
            private readonly int _threshold;

            public ContentFilter(int threshold)
            {
                _threshold = threshold;
            }

            public bool IsMatch(object? value)
            {
                if (value == null)
                    return false;

                return int.TryParse(value.ToString(), out var i) && (i > _threshold);
            }

            public string Value => _threshold.ToString(CultureInfo.CurrentCulture);
        }
    }
}
