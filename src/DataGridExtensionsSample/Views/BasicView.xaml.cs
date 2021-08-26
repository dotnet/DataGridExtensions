namespace DataGridExtensionsSample.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;

    using DataGridExtensions;

    using TomsToolbox.Wpf.Composition.AttributedModel;

    /// <summary>
    /// Interaction logic for BasicView.xaml
    /// </summary>
    [DataTemplate(typeof(BasicViewModel))]
    public partial class BasicView
    {
        public BasicView()
        {
            InitializeComponent();

            // must defer action, filters not yet available in OnLoaded.
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                // Sample to manipulate the filter values by code.
                DataGrid.Columns[0].SetFilter("True");
                DataGrid.Columns[2].SetFilter("3");
                DataGrid.Columns[3].SetIsFilterVisible(false);
            }));

            // Sample usage of the filtering event
            DataGrid.GetFilter().Filtering += DataGrid_Filtering;
        }

        private void DataGrid_Filtering(object? sender, DataGridFilteringEventArgs e)
        {
            // Here we could prepare some data or even cancel the filtering process.

            Dispatcher.BeginInvoke(new Action(DataGrid_Filtered));
        }

        private void DataGrid_Filtered()
        {
            // Here we could show some information about the result of the filtering.

            Trace.WriteLine(DataGrid.Items.Count);
        }



    }
}
