namespace DataGridExtensionsSample
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    using DataGridExtensions;

    /// <summary>
    /// Interaction logic for MultipleChoiceFilter.xaml
    /// </summary>
    public partial class MultipleChoiceFilter
    {
        private ListBox _listBox;

        public MultipleChoiceFilter()
        {
            InitializeComponent();
        }


        public MultipleChoiceContentFilter Filter
        {
            get { return (MultipleChoiceContentFilter)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }
        /// <summary>
        /// Identifies the Filter dependency property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(MultipleChoiceContentFilter), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(new MultipleChoiceContentFilter(null), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((MultipleChoiceFilter)sender).Filter_Changed()));



        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = Template.FindName("ListBox", this) as ListBox;

            var filter = Filter;

            if (filter?.Items == null)
            {
                _listBox?.SelectAll();
            }

            var items = _listBox?.Items as INotifyCollectionChanged;
            if (items == null)
                return;

            items.CollectionChanged += ListBox_ItemsCollectionChanged;
        }

        private void ListBox_ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var filter = Filter;

            if (filter?.Items == null)
            {
                _listBox?.SelectAll();
            }
        }

        private void Filter_Changed()
        {
            var filter = Filter;

            if (filter?.Items == null)
            {
                _listBox?.SelectAll();
                return;
            }

            if (_listBox?.SelectedItems.Count != 0)
                return;

            foreach (var item in filter.Items)
            {
                _listBox.SelectedItems.Add(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Filter = new MultipleChoiceContentFilter(_listBox?.SelectedItems.Cast<string>());
        }
    }
    public class MultipleChoiceContentFilter : IContentFilter
    {
        public MultipleChoiceContentFilter(IEnumerable<string> items)
        {
            Items = items?.ToArray();
        }

        public IList<string> Items
        {
            get;
        }

        public bool IsMatch(object value)
        {
            return Items?.Contains(value as string) ?? true;
        }
    }
}
