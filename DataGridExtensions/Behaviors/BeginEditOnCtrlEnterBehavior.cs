namespace DataGridExtensions.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    /// <summary>
    /// Ctrl+Enter on a cell starts editing the cell without clearing the content.
    /// </summary>
    public class BeginEditOnCtrlEnterBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewKeyDown += DataGrid_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewKeyDown -= DataGrid_PreviewKeyDown;
        }

        private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var dependencyObject = e.OriginalSource as DependencyObject;

            if (IsChildOfEditingElement(dependencyObject))
                return;

            var key = e.Key;
            if ((key != Key.Return) || (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
                return;

            var dataGrid = (DataGrid)sender;
            dataGrid.BeginEdit();
            e.Handled = true;
        }

        private static bool IsChildOfEditingElement(DependencyObject element)
        {
            while (element != null)
            {
                if (element is TextBox)
                    return true;

                if (element is DataGrid)
                    return false;

                element = LogicalTreeHelper.GetParent(element);
            }

            return false;
        }
    }
}
