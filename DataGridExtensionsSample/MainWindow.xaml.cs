using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DataGridExtensions;

namespace DataGridExtensionsSample
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Data;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private object _column2Filter = "A";
        private const char TextColumnSeparator = '\t';

        public MainWindow()
        {
            InitializeComponent();

            var xs = Enumerable.Range(0, 100).Select(index => new DataItem(_rand, index)).ToArray();
            Task.Factory
                .StartNew(() => Thread.Sleep(3000))
                .ContinueWith(task =>
                {
                    Items = xs;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
                });
            
            // Sample usage of the filtering event
            Grid1.GetFilter().Filtering += Grid1_Filtering;

            ExternalFilter = item => ((DataItem)item).Column1.Contains("7");

            BindingOperations.SetBinding(Column2, DataGridFilterColumn.FilterProperty, new Binding("DataContext.Column2Filter") { Source = this, Mode = BindingMode.TwoWay });
        }

        public object Column2Filter
        {
            get
            {
                return _column2Filter;
            }
            set
            {
                _column2Filter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Column2Filter)));
            }
        }

        void Grid1_Filtering(object sender, DataGridFilteringEventArgs e)
        {
            // Here we could prepare some data or even cancel the filtering process.

            Dispatcher.BeginInvoke(new Action(Grid1_Filtered));
        }

        void Grid1_Filtered()
        {
            // Here we could show some information about the result of the filtering.

            Trace.WriteLine(Grid1.Items.Count);
        }

        /// <summary>
        /// Provide a simple list of 100 random items.
        /// </summary>
        public IEnumerable<DataItem> Items { get; private set; }

        public Predicate<object> ExternalFilter
        {
            get { return (Predicate<object>)GetValue(ExternalFilterProperty); }
            set { SetValue(ExternalFilterProperty, value); }
        }

        public static readonly DependencyProperty ExternalFilterProperty = DependencyProperty.Register("ExternalFilter", typeof(Predicate<object>), typeof(MainWindow));


        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Use the integer filter for the integer column.
            if (e.PropertyType == typeof(int))
            {
                e.Column.SetTemplate((ControlTemplate)FindResource("IntegerFilter"));
            }
            else if ((e.PropertyType != typeof(bool)) && e.PropertyType.IsPrimitive)
            {
                // Hide the filter for all other primitive data types except bool.
                // Here we will hide the filter for the double DataItem.Probability.
                e.Column.SetIsFilterVisible(false);
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // must defer action, filters not yet available in OnLoaded.
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                // Sample to manipulate the filter values by code.
            }));
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Items"));
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (!CopyPasteDataGrid.HasRectangularCellSelection())
            {
                MessageBox.Show("Invalid selection for copy");
                return;
            }

            var cellSelection = CopyPasteDataGrid.GetCellSelection();

            Clipboard.SetText(cellSelection.ToString(TextColumnSeparator));
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            CopyPasteDataGrid.PasteCells(Clipboard.GetText().ParseTable(TextColumnSeparator));
        }

        private void ClearAllFilters_Click(object sender, RoutedEventArgs e)
        {
            Grid1.GetFilter().Clear();
        }

        private void ClearIpsum_Click(object sender, RoutedEventArgs e)
        {
            var ipsumItems = Items.Where(item => item.Column5 == "ipsum").ToList();

            foreach (var item in ipsumItems)
            {
                Items.Remove(item);
            }
        }
    }

    internal static class ExtensionMethods
    {
        private const string Quote = "\"";

        internal static string ToString(this IList<IList<string>> table, char separator)
        {
            if ((table.Count == 1) && (table[0] != null) && (table[0].Count == 1) && string.IsNullOrWhiteSpace(table[0][0]))
                return Quote + (table[0][0] ?? string.Empty) + Quote;

            return string.Join(Environment.NewLine, table.Select(line => string.Join(separator.ToString(), line.Select(cell => Quoted(cell, separator)))));
        }

        internal static IList<IList<string>> ParseTable(this string text, char separator)
        {
            var table = new List<IList<string>>();

            using (var reader = new StringReader(text))
            {
                while (reader.Peek() != -1)
                {
                    table.Add(ReadTableLine(reader, separator));
                }
            }

            if (!table.Any())
                return null;

            var headerColumns = table.First();

            return table.Any(columns => columns.Count != headerColumns.Count) ? null : table;
        }

        internal static IList<string> ReadTableLine(TextReader reader, char separator)
        {
            var columns = new List<string>();

            while (true)
            {
                columns.Add(ReadTableColumn(reader, separator));

                if ((char)reader.Peek() == separator)
                {
                    reader.Read();
                    continue;
                }

                while (IsLineFeed(reader.Peek()))
                {
                    reader.Read();
                }

                break;
            }

            return columns;
        }

        internal static string ReadTableColumn(TextReader reader, char separator)
        {
            var stringBuilder = new StringBuilder();
            int nextChar;

            if (IsDoubleQuote(reader.Peek()))
            {
                reader.Read();

                while ((nextChar = reader.Read()) != -1)
                {
                    if (IsDoubleQuote(nextChar))
                    {
                        if (IsDoubleQuote(reader.Peek()))
                        {
                            reader.Read();
                            stringBuilder.Append((char)nextChar);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        stringBuilder.Append((char)nextChar);
                    }
                }
            }
            else
            {
                while ((nextChar = reader.Peek()) != -1)
                {
                    if (IsLineFeed(nextChar) || (nextChar == separator))
                        break;

                    reader.Read();
                    stringBuilder.Append((char)nextChar);
                }
            }

            return stringBuilder.ToString();
        }

        private static bool IsDoubleQuote(int c)
        {
            return (c == '"');
        }


        internal static string Quoted(string value, char separator)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.Any(IsLineFeed) || value.Contains(separator) || value.StartsWith(Quote, StringComparison.Ordinal))
            {
                return Quote + value.Replace(Quote, Quote + Quote) + Quote;
            }

            return value;
        }

        private static bool IsLineFeed(int c)
        {
            return (c == '\r') || (c == '\n');
        }

        private static bool IsLineFeed(char c)
        {
            return IsLineFeed((int)c);
        }
    }
}
