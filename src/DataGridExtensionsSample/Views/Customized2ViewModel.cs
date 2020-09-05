namespace DataGridExtensionsSample.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    using TomsToolbox.Essentials;
    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    [VisualCompositionExport(RegionId.Main, Sequence = 3)]
    [DisplayName("Customized 2")]
    class Customized2ViewModel : ObservableObject
    {
        public Customized2ViewModel(DataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public DataProvider DataProvider { get; }

        public object Column2Filter { get; set; } = "A";

        public Predicate<object> GlobalFilter { get; } = item => (item as DataItem)?.Column1?.Contains("7") ?? false;

        public ICommand ClearIpsumCommand => new DelegateCommand(ClearIpsum);

        private void ClearIpsum()
        {
            DataProvider.Items.RemoveWhere(item => item.Column5 == "ipsum");
        }
    }
}
