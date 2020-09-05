namespace DataGridExtensionsSample.Views
{
    using System.Composition;

    using DataGridExtensionsSample.Infrastructure;

    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    [VisualCompositionExport(RegionId.Shell)]
    [Shared]
    public class MainViewModel : ObservableObject
    {
        public MainViewModel(DataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public DataProvider DataProvider { get; }

    }
}
