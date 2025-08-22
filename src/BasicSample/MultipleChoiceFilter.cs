namespace BasicSample;

using System.Windows;

/// <summary>
/// Interaction logic for MultipleChoiceFilter.xaml
/// </summary>
public sealed class MultipleChoiceFilter : DataGridExtensions.MultipleChoiceFilter
{
    static MultipleChoiceFilter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultipleChoiceFilter), new FrameworkPropertyMetadata(typeof(DataGridExtensions.MultipleChoiceFilter)));
    }

    public MultipleChoiceFilter()
    {
        SelectAllContent = "Alle!";
        HasTextFilter = true;
    }
}
