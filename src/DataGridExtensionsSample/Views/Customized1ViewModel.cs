namespace DataGridExtensionsSample.Views
{
    using System.ComponentModel;
    using System.Windows.Input;

    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    [VisualCompositionExport(RegionId.Main, Sequence = 2)]
    [DisplayName("Customized 1")]
    class Customized1ViewModel : ObservableObject
    {
        public Customized1ViewModel(DataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public DataProvider DataProvider { get; }

        public ICommand ReloadCommand => new DelegateCommand(Reload);
        
        private void Reload()
        {
            DataProvider.Reload();
        }
    }
}
