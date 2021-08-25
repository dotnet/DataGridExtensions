namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Text.RegularExpressions;
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
        private DataGrid? _dataGrid;
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
            DependencyProperty.Register("Filter", typeof(MultipleChoiceContentFilter), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, _) => ((MultipleChoiceFilter)sender).Filter_Changed()));

        public bool HasTextFilter
        {
            get => (bool)GetValue(HasTextFilterProperty);
            set => SetValue(HasTextFilterProperty, value);
        }
        public static readonly DependencyProperty HasTextFilterProperty = DependencyProperty.Register(
            "HasTextFilter", typeof(bool), typeof(MultipleChoiceFilter), new PropertyMetadata(default(bool)));

        private IEnumerable<string?>? SourceValues => (IEnumerable<string?>?)GetValue(SourceValuesProperty);

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
            "SelectAllContent", typeof(object), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata("(Select All)"));

        /// <summary>
        /// Gets or sets an optional text to pre-filter the list
        /// </summary>
        public string? Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        /// <summary>
        /// Defines the Text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (sender, e) => ((MultipleChoiceFilter)sender).Text_Changed()));

        private void Text_Changed()
        {
            Filter = CreateFilter(Filter?.Items, Text);
            OnSourceValuesChanged(SourceValues);
        }

        /// <summary>
        /// Gets or sets a value that controls if the popup is open.
        /// </summary>
        public bool IsPopupOpen
        {
            get => (bool)GetValue(IsPopupOpenProperty);
            set => SetValue(IsPopupOpenProperty, value);
        }
        /// <summary>
        /// The is popup open property
        /// </summary>
        public static readonly DependencyProperty IsPopupOpenProperty = DependencyProperty.Register(
            "IsPopupOpen", typeof(bool), typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(default(bool), (sender, e) => ((MultipleChoiceFilter)sender).IsPopupOpen_Changed((bool)e.NewValue)));

        private void IsPopupOpen_Changed(bool newValue)
        {
            if (!newValue)
            {
                this.MoveFocusToDataGrid(_dataGrid);
            }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var listBox = _listBox = Template?.FindName("PART_ListBox", this) as ListBox;
            if (listBox == null)
                return;
            listBox.SelectionChanged += ListBox_SelectionChanged;

            var filterColumnControl = this.FindAncestorOrSelf<DataGridFilterColumnControl>();

            BindingOperations.SetBinding(this, FilterProperty, new Binding { Source = filterColumnControl, Path = new PropertyPath(DataGridFilterColumnControl.FilterProperty) });
            BindingOperations.SetBinding(this, SourceValuesProperty, new Binding { Source = filterColumnControl, Path = new PropertyPath(nameof(DataGridFilterColumnControl.SelectableValues)) });

            _dataGrid = filterColumnControl?.FindAncestorOrSelf<DataGrid>();
            if (_dataGrid == null)
                return;

            var dataGridItems = (INotifyCollectionChanged)_dataGrid.Items;
            dataGridItems.CollectionChanged += (_, _) => UpdateSourceValuesTarget();

            _dataGrid.SetTrackFocusedCell(true);

            var filter = Filter;

            if (filter?.Items == null)
            {
                listBox.SelectAll();
            }

            var items = (INotifyCollectionChanged)listBox.Items;

            items.CollectionChanged += ListBox_ItemsCollectionChanged;
        }

        /// <summary>
        /// Creates the filter.
        /// </summary>
        /// <param name="items">The items to filter.</param>
        /// <param name="text">The optional text to pre-filter the items.</param>
        /// <returns>The filter.</returns>
        protected virtual MultipleChoiceContentFilter CreateFilter(IEnumerable<string?>? items, string? text = null)
        {
            return new MultipleChoiceContentFilter(items, text);
        }

        /// <summary>
        /// Called when the source values have changed.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSourceValuesChanged(IEnumerable<string?>? newValue)
        {
            var values = Values;
            var filterRegex = Filter?.Regex;

            if (newValue == null)
            {
                values.Clear();
            }
            else
            {
                values.SynchronizeWith(newValue.ExceptNullItems()
                    .Where(item => filterRegex?.IsMatch(item) != false)
                    .ToArray());
            }
        }

        /// <inheritdoc />
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            if (true.Equals(e.NewValue))
            {
                this.BeginInvoke(() => IsPopupOpen = true);
            }
        }

        private void ListBox_ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
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

            Text = filter?.Text;

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

            Filter = CreateFilter(areAllItemsSelected ? null : selectedItems, Text);
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
        /// <param name="text">The optional regex to filter</param>
        public MultipleChoiceContentFilter(IEnumerable<string?>? items, string? text = null)
        {
            Text = text;
            try
            {
                Regex = text.IsNullOrWhiteSpace() ? null : new Regex(text, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                // invalid user input, just go with a null expression.
            }

            Items = items != null ? new HashSet<string?>(items.Where(item => Regex?.IsMatch(item) != false)) : null;
        }

        /// <summary>
        /// Gets the items to filter.
        /// </summary>
        public ICollection<string?>? Items { get; }

        /// <summary>
        ///  Gets the text to filter.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// Gets the text regex to filter.
        /// </summary>
        public Regex? Regex { get; }

        /// <inheritdoc />
        public virtual bool IsMatch(object? value)
        {
            var input = value?.ToString();
            var items = Items;

            if (items == null)
                return Regex?.IsMatch(input) ?? true;

            if (string.IsNullOrWhiteSpace(input))
                return items.Contains(string.Empty);

            return items.ContainsAny(input);
        }
    }
}
