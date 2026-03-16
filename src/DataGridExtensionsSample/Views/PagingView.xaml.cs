namespace DataGridExtensionsSample.Views
{
    using TomsToolbox.Wpf.Composition.AttributedModel;

    /// <summary>
    /// Interaction logic for PagingView.xaml
    /// </summary>
    [DataTemplate(typeof(PagingViewModel))]
    public partial class PagingView
    {
        public PagingView()
        {
            InitializeComponent();
        }
    }
}

