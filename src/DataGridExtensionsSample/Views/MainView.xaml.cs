namespace DataGridExtensionsSample.Views;

using TomsToolbox.Wpf.Composition.AttributedModel;

/// <summary>
/// Interaction logic for MainView.xaml
/// </summary>
[DataTemplate(typeof(MainViewModel))]
public partial class MainView
{
    public MainView()
    {
        InitializeComponent();
    }
}
