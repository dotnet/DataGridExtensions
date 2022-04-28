namespace DataGridExtensionsSample.Views
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Input;

    using DataGridExtensions;

    using DataGridExtensionsSample.Controls;
    using DataGridExtensionsSample.Infrastructure;

    using TomsToolbox.Essentials;
    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition.AttributedModel;

    [VisualCompositionExport(RegionId.Main, Sequence = 3)]
    [DisplayName("Customized 2")]
    internal class Customized2ViewModel : ObservableObject
    {
        public Customized2ViewModel(DataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public ObservableCollection<DataItem> SelectedItems { get; } = new ObservableCollection<DataItem>();

        public DataProvider DataProvider { get; }

        public object Column2Filter { get; set; } = "A";

        public object? Column5Filter { get; set; }

        public object ColumnTextWithPrefilterFilter { get; set; } = new MultipleChoiceContentFilter(new List<string> { "amet" });

        public bool Column5PopupVisible { get; set; }

        public DataGridFilterColumnControl? Column5FilterColumnControl { get; set; }

#pragma warning disable CA1307 // Specify StringComparison for clarity => only supported in net5.0!
        public Predicate<object> GlobalFilter { get; } = item => (item as DataItem)?.Column1?.Contains('7') ?? false;
#pragma warning restore CA1307 // Specify StringComparison for clarity


        public ICommand ClearFilterCommand => new DelegateCommand<DataGrid>(ClearFilter);

        private void ClearFilter(DataGrid? dataGrid)
        {
            dataGrid?.GetFilter().Clear();
        }

        public ICommand ClearIpsumCommand => new DelegateCommand(ClearIpsum);

        private void ClearIpsum()
        {
            DataProvider.Items.RemoveWhere(item => item.Column5 == "ipsum");
        }

        public ICommand OpenAndPopulateAFilterCommand => new DelegateCommand(OpenAndPopulateAFilter);

        private void OpenAndPopulateAFilter()
        {
            Column5Filter = new ContentFilter(0.5d, 1d);
            if (Column5FilterColumnControl?.FilterControl is FilterWithPopupControl filterWithPopupControl)
            {
                filterWithPopupControl.IsPopupVisible = true;
            }
        }

        public ICommand ProgrammaticAccessToFilterControlCommand => new DelegateCommand(ProgrammaticAccessToFilterControl);

        private void ProgrammaticAccessToFilterControl()
        {
            if (Column5FilterColumnControl?.FilterControl is FilterWithPopupControl filterWithPopupControl)
            {
                filterWithPopupControl.Caption = "New Popup Caption:";
                filterWithPopupControl.IsPopupVisible = true;
            }
        }
    }
}
