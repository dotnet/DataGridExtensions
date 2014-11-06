namespace DataGridExtensions
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    internal static class ApplyInitialSorting
    {
        public static void IsEnabled_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            if ((bool)e.NewValue)
                dataGrid.Loaded += DataGrid_Loaded;
            else
                dataGrid.Loaded -= DataGrid_Loaded;
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid == null)
                return;

            foreach (var column in dataGrid.Columns)
            {
                if ((column != null) && (column.SortDirection.HasValue) && !string.IsNullOrEmpty(column.SortMemberPath))
                {
                    dataGrid.Items.SortDescriptions.Add(new SortDescription(column.SortMemberPath, column.SortDirection.Value));
                }
            }
        }            

    }
}
