namespace DataGridExtensionsSample.Views;

using TomsToolbox.Wpf.Composition.AttributedModel;

/// <summary>
/// Interaction logic for GroupingView.xaml
/// </summary>
[DataTemplate(typeof(GroupingViewModel))]
public partial class GroupingView 
{
    public GroupingView()
    {
        InitializeComponent();
    }
}
