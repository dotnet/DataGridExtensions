namespace DataGridExtensions
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    using Throttle;

    using TomsToolbox.Essentials;
    using TomsToolbox.Wpf;

    /// <summary>
    /// Base class to implement a multiple choice filter.
    /// </summary>
    [TemplatePart(Name = "PART_ListBox", Type = typeof(ListBox))]
    public class MultipleChoiceFilter : Control
    {
        private ListBox? _listBox;

        static MultipleChoiceFilter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(typeof(MultipleChoiceFilter)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleChoiceFilter"/> class.
        /// </summary>
        public MultipleChoiceFilter()
        {
            Values = new ObservableCollection<string>();
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        public MultipleChoiceContentFilter? Filter
        {
            get => (MultipleChoiceContentFilter)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }
        /// <summary>
        /// The filter property
        /// </summary>
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(MultipleChoiceContentFilter), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((MultipleChoiceFilter)sender).Filter_Changed()));

        private static readonly DependencyProperty SourceValuesProperty =
            DependencyProperty.Register("SourceValues", typeof(IList<string>), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(null, (sender, e) => ((MultipleChoiceFilter)sender).SourceValues_Changed((IList<string>)e.NewValue)));

        private void SourceValues_Changed(IEnumerable<string?>? newValue)
        {
            OnSourceValuesChanged(newValue);
        }

        /// <summary>
        /// Gets the values.
        /// </summary>
        public IList<string> Values
        {
            get => (IList<string>)GetValue(ValuesProperty);
            private set => SetValue(ValuesPropertyKey, value);
        }
        private static readonly DependencyPropertyKey ValuesPropertyKey =
            DependencyProperty.RegisterReadOnly("Values", typeof(IList<string>), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata());
        /// <summary>
        /// The values property
        /// </summary>
        public static readonly DependencyProperty ValuesProperty = ValuesPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the content of the select all check box; default is "(Select All)"
        /// </summary>
        public object SelectAllContent
        {
            get => GetValue(SelectAllContentProperty);
            set => SetValue(SelectAllContentProperty, value);
        }
        /// <summary>
        /// The select all content property
        /// </summary>
        public static readonly DependencyProperty SelectAllContentProperty = DependencyProperty.Register(
            "SelectAllContent", typeof(object), typeof(MultipleChoiceFilter), new PropertyMetadata("(Select All)"));

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var filterColumnControl = this.FindAncestorOrSelf<DataGridFilterColumnControl>();

            BindingOperations.SetBinding(this, FilterProperty, new Binding { Source = filterColumnControl, Path = new PropertyPath(DataGridFilterColumnControl.FilterProperty) });
            BindingOperations.SetBinding(this, SourceValuesProperty, new Binding { Source = filterColumnControl, Path = new PropertyPath(nameof(DataGridFilterColumnControl.SelectableValues)) });

            var dataGrid = filterColumnControl?.FindAncestorOrSelf<DataGrid>();
            if (dataGrid == null)
                return;

            var dataGridItems = (INotifyCollectionChanged)dataGrid.Items;
            dataGridItems.CollectionChanged += (_, __) => UpdateSourceValuesTarget();

            var listBox = _listBox = Template?.FindName("PART_ListBox", this) as ListBox;
            if (listBox == null)
                return;

            var filter = Filter;

            if (filter?.Items == null)
            {
                listBox.SelectAll();
            }

            listBox.SelectionChanged += ListBox_SelectionChanged;
            var items = (INotifyCollectionChanged)listBox.Items;

            items.CollectionChanged += ListBox_ItemsCollectionChanged;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <returns>The filter.</returns>
        protected virtual MultipleChoiceContentFilter CreateFilter(IEnumerable<string?>? items)
        {
            return new MultipleChoiceContentFilter(items);
        }

        /// <summary>
        /// Called when the source values have changed.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSourceValuesChanged(IEnumerable<string?>? newValue)
        {
            var values = Values;

            if (newValue == null)
                values.Clear();
            else
                values.SynchronizeWith(newValue.ExceptNullItems().ToArray());
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
            var listBox = _listBox;
            if (listBox == null)
                return;

            var filter = Filter;
            if (filter?.Items == null)
            {
                listBox.SelectAll();
                return;
            }

            if (listBox.SelectedItems.Count != 0)
                return;

            foreach (var item in filter.Items)
            {
                listBox.SelectedItems.Add(item);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;

            var selectedItems = listBox.SelectedItems.Cast<string>().ToArray();

            var areAllItemsSelected = listBox.Items.Count == selectedItems.Length;

            Filter = CreateFilter(areAllItemsSelected ? null : selectedItems);
        }

        [Throttled(typeof(DispatcherThrottle))]
        private void UpdateSourceValuesTarget()
        {
            BindingOperations.GetBindingExpression(this, SourceValuesProperty)?.UpdateTarget();
        }
    }

    /// <summary>
    /// Base class for a multiple choice content filter
    /// </summary>
    public class MultipleChoiceContentFilter : IContentFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultipleChoiceContentFilter"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public MultipleChoiceContentFilter(IEnumerable<string?>? items)
        {
            Items = items?.ToArray();
        }

        /// <summary>
        /// Gets the items to filter.
        /// </summary>
        public IList<string?>? Items
        {
            get;
        }

        /// <inheritdoc />
        public virtual bool IsMatch(object? value)
        {
            var input = value?.ToString();
            if (string.IsNullOrWhiteSpace(input))
                return Items?.Contains(string.Empty) ?? true;

            return Items?.ContainsAny(input) ?? true;
        }
    }
}
