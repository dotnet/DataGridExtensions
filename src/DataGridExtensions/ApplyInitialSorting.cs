namespace DataGridExtensions
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    internal static class ApplyInitialSorting
    {
        public static void IsEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DataGrid dataGrid))
                return;

            if (true.Equals(e.NewValue))
                dataGrid.Loaded += DataGrid_Loaded;
            else
                dataGrid.Loaded -= DataGrid_Loaded;
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is DataGrid dataGrid))
                return;

            try
            {
                dataGrid.Items.SortDescriptions.Clear();

                foreach (var column in dataGrid.Columns)
                {
                    if (column?.SortDirection != null && !string.IsNullOrEmpty(column.SortMemberPath))
                    {
                        dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, column.SortDirection.Value));
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // in some special cases we may get:
                // System.InvalidOperationException: 'Sorting' is not allowed during an AddNew or EditItem transaction.
                // => just ignore, there is nothing we can do about it...
            }
        }
    }
}
