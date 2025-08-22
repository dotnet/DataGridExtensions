namespace DataGridExtensionsSample.Views;

using System.ComponentModel;

using DataGridExtensionsSample.Infrastructure;

using TomsToolbox.Wpf;
using TomsToolbox.Wpf.Composition.AttributedModel;

[VisualCompositionExport(RegionId.Main, Sequence = 5)]
[DisplayName("Grouping")]
internal sealed class GroupingViewModel : ObservableObject
{
    public GroupingViewModel(DataProvider dataProvider)
    {
        DataProvider = dataProvider;
    }

    public DataProvider DataProvider { get; }
}
