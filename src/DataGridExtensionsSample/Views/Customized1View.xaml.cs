namespace DataGridExtensionsSample.Views;

using System.Windows.Controls;

using DataGridExtensions;

using TomsToolbox.Wpf.Composition.AttributedModel;

/// <summary>
/// Interaction logic for Customized1View.xaml
/// </summary>
[DataTemplate(typeof(Customized1ViewModel))]
public partial class Customized1View 
{
    public Customized1View()
    {
        InitializeComponent();
    }

    private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
    {
        // Use the integer filter for the integer column.
        if (e.PropertyType == typeof(int))
        {
            e.Column.SetTemplate((ControlTemplate)FindResource("IntegerFilter"));
        }
        else if ((e.PropertyType != typeof(bool)) && e.PropertyType.IsPrimitive)
        {
            // Hide the filter for all other primitive data types except bool.
            // Here we will hide the filter for the double DataItem.Probability.
            e.Column.SetIsFilterVisible(false);
        }
    }
}
