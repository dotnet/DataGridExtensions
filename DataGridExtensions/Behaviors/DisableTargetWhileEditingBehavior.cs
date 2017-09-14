namespace DataGridExtensions.Behaviors
{
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interactivity;

    using JetBrains.Annotations;

    /// <summary>
    /// This behavior disables the specified <see cref="Target"/> element while the
    /// DataGrid is in editing mode.
    /// </summary>
    public class DisableTargetWhileEditingBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// Gets or sets the target element that will be disabled during editing.
        /// </summary>
        [CanBeNull]
        public UIElement Target
        {
            get => (UIElement)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }
        /// <summary>
        /// Identifies the Target dependency property
        /// </summary>
        [NotNull]
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(UIElement), typeof(DisableTargetWhileEditingBehavior));

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            var dataGrid = AssociatedObject;
            Contract.Assume(dataGrid != null);

            dataGrid.PreparingCellForEdit += DataGrid_PreparingCellForEdit;
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            var dataGrid = AssociatedObject;
            Contract.Assume(dataGrid != null);

            dataGrid.PreparingCellForEdit -= DataGrid_PreparingCellForEdit;
            dataGrid.CellEditEnding -= DataGrid_CellEditEnding;
        }

        private void DataGrid_CellEditEnding([NotNull] object sender, [NotNull] DataGridCellEditEndingEventArgs e)
        {
            if (Target == null)
                return;

            Target.IsEnabled = true;
        }

        private void DataGrid_PreparingCellForEdit([NotNull] object sender, [NotNull] DataGridPreparingCellForEditEventArgs e)
        {
            if (Target == null)
                return;

            Target.IsEnabled = false;
        }
    }
}
