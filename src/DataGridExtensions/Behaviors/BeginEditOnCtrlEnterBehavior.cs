namespace DataGridExtensions.Behaviors;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Xaml.Behaviors;

/// <summary>
/// Ctrl+Enter on a cell starts editing the cell without clearing the content.
/// </summary>
public class BeginEditOnCtrlEnterBehavior : Behavior<DataGrid>
{
    /// <summary>
    /// Called after the behavior is attached to an AssociatedObject.
    /// </summary>
    /// <remarks>
    /// Override this to hook up functionality to the AssociatedObject.
    /// </remarks>
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.PreviewKeyDown += DataGrid_PreviewKeyDown;
    }

    /// <summary>
    /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
    /// </summary>
    /// <remarks>
    /// Override this to unhook functionality from the AssociatedObject.
    /// </remarks>
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

        (sender as DataGrid)?.BeginEdit();

        e.Handled = true;
    }

    private static bool IsChildOfEditingElement(DependencyObject? element)
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
