using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using System.Windows.Controls;

namespace DataGridExtensions
{
    public static class Commands
    {
       public static ICommand ClearComboBoxCommand => new ActionCommand((control) =>
           {
               ((ComboBox) control).SelectedItem = null;
           });
    }
}
