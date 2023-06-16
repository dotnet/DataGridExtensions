namespace DataGridExtensionsSample.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using DataGridExtensions;

    using DataGridExtensionsSample.Infrastructure;

    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    [VisualCompositionExport(RegionId.Main, Sequence = 4)]
    [DisplayName("Copy/Paste")]
    internal sealed class CopyPasteViewModel : ObservableObject
    {
        private const char TextColumnSeparator = '\t';

        public CopyPasteViewModel(DataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public DataProvider DataProvider { get; }

        public ICommand CopyCommand => new DelegateCommand<DataGrid>(Copy);

        public ICommand PasteCommand => new DelegateCommand<DataGrid>(Paste);

        private void Copy(DataGrid dataGrid)
        {
            if (!dataGrid.HasRectangularCellSelection())
            {
                MessageBox.Show("Invalid selection for copy");
                return;
            }

            var cellSelection = dataGrid.GetCellSelection();

            Clipboard.SetText(cellSelection?.ToString(TextColumnSeparator));
            
        }

        private void Paste(DataGrid dataGrid)
        {
            if (!dataGrid.PasteCells(Clipboard.GetText().ParseTable(TextColumnSeparator)))
            {
                MessageBox.Show("Selection does not match data.");
            }
        }
    }
}
