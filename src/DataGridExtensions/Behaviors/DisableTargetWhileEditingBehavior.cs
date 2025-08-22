namespace DataGridExtensions.Behaviors;

using System.Windows;
using System.Windows.Controls;

using Microsoft.Xaml.Behaviors;

/// <summary>
/// This behavior disables the specified <see cref="Target"/> element while the
/// DataGrid is in editing mode.
/// </summary>
public class DisableTargetWhileEditingBehavior : Behavior<DataGrid>
{
    /// <summary>
    /// Gets or sets the target element that will be disabled during editing.
    /// </summary>
    public UIElement? Target
    {
        get => (UIElement)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }
    /// <summary>
    /// Identifies the Target dependency property
    /// </summary>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(DisableTargetWhileEditingBehavior));

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

        dataGrid.PreparingCellForEdit += DataGrid_PreparingCellForEdit;
        dataGrid.CellEditEnding += DataGrid_CellEditEnding;
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

        dataGrid.PreparingCellForEdit -= DataGrid_PreparingCellForEdit;
        dataGrid.CellEditEnding -= DataGrid_CellEditEnding;
    }

    private void DataGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (Target == null)
            return;

        Target.IsEnabled = true;
    }

    private void DataGrid_PreparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
    {
        if (Target == null)
            return;

        Target.IsEnabled = false;
    }
}
