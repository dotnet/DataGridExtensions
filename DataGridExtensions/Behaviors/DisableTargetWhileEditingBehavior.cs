namespace DataGridExtensions.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;

    /// <summary>
    /// This behavior disables the specified <see cref="Target"/> element while the 
    /// DataGrid is in editing mode.
    /// </summary>
    public class DisableTargetWhileEditingBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// Gets or sets the target element that will be disabled during editing.
        /// </summary>
        public UIElement Target
        {
            get { return (UIElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        /// <summary>
        /// Identifies the Target dependency property
        /// </summary>
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof (UIElement), typeof (DisableTargetWhileEditingBehavior));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreparingCellForEdit += DataGrid_PreparingCellForEdit;
            AssociatedObject.CellEditEnding += DataGrid_CellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreparingCellForEdit -= DataGrid_PreparingCellForEdit;
            AssociatedObject.CellEditEnding -= DataGrid_CellEditEnding;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Target == null)
                return;

            Target.IsEnabled = true;
        }

        private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            if (Target == null)
                return;

            Target.IsEnabled = false;
        }
    }
}
