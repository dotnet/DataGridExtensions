namespace DataGridExtensions.Behaviors
{
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interactivity;

    using JetBrains.Annotations;

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

            var dataGrid = AssociatedObject;
            Contract.Assume(dataGrid != null);

            dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
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

            var dataGrid = AssociatedObject;
            Contract.Assume(dataGrid != null);

            dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
        }

        private static void DataGrid_PreviewKeyDown([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Contract.Requires(sender != null);

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

        private static bool IsChildOfEditingElement([CanBeNull] DependencyObject element)
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
