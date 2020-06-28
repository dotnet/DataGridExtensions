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
        private ListBox? _listBox;

        public MultipleChoiceFilter()
        {
            InitializeComponent();
        }


        public MultipleChoiceContentFilter Filter
        {
            get => (MultipleChoiceContentFilter)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
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

            if (filter?.ExcludedItems == null)
            {
                _listBox?.SelectAll();
            }

            if (!(_listBox?.Items is INotifyCollectionChanged items))
                return;

            items.CollectionChanged += ListBoxItems_CollectionChanged;
        }

        private void ListBoxItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var filter = Filter;

            var listBox = _listBox!;

            if (filter?.ExcludedItems == null)
            {
                listBox.SelectAll();
            }
            else
            {
                foreach (var item in listBox.Items.Cast<string>().Except(filter.ExcludedItems))
                {
                    listBox.SelectedItems.Add(item);
                }
            }
        }

        private void Filter_Changed()
        {
            var filter = Filter;

            if (filter?.ExcludedItems == null)
            {
                _listBox?.SelectAll();
                return;
            }

            if (_listBox?.SelectedItems.Count != 0)
                return;

            foreach (var item in _listBox.Items.Cast<string>().Except(filter.ExcludedItems))
            {
                _listBox.SelectedItems.Add(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var excludedItems = Filter?.ExcludedItems ?? Enumerable.Empty<string>();

            var listBox = (ListBox)sender;
            var selectedItems = listBox.SelectedItems.Cast<string>().ToArray();
            var unselectedItems = listBox.Items.Cast<string>().Except(selectedItems).ToArray();

            excludedItems = excludedItems.Except(selectedItems).Concat(unselectedItems).Distinct().ToList();

            Filter = new MultipleChoiceContentFilter(excludedItems);
        }
    }
    public class MultipleChoiceContentFilter : IContentFilter
    {
        public MultipleChoiceContentFilter(IEnumerable<string>? excludedItems)
        {
            ExcludedItems = excludedItems?.ToArray();
        }

        public IList<string>? ExcludedItems
        {
            get;
        }

        public bool IsMatch(object? value)
        {
            return ExcludedItems?.Contains(value?.ToString() ?? string.Empty) != true;
        }
    }
}
