namespace DataGridExtensionsSample.Views
{
    using TomsToolbox.Wpf.Composition.AttributedModel;

    /// <summary>
    /// Interaction logic for CopyPasteView.xaml
    /// </summary>
    [DataTemplate(typeof(CopyPasteViewModel))]
    public partial class CopyPasteView 
    {
        public CopyPasteView()
        {
            InitializeComponent();
        }
    }
}
