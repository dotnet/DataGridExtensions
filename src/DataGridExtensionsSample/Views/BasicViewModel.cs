namespace DataGridExtensionsSample.Views;

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

using DataGridExtensions;

using DataGridExtensionsSample.Infrastructure;

using TomsToolbox.Wpf;
using TomsToolbox.Wpf.Composition.AttributedModel;

[VisualCompositionExport(RegionId.Main, Sequence = 1)]
[DisplayName("Basic usage")]
internal sealed class BasicViewModel : ObservableObject
{
    public BasicViewModel(DataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public DataProvider DataProvider { get; }

    public ICommand ClearAllFiltersCommand => new DelegateCommand<DataGrid>(ClearAllFilters);

    private void ClearAllFilters(DataGrid? dataGrid)
    {
        dataGrid?.GetFilter().Clear();
    }
}
