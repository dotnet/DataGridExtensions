### Behaviors
The extension provides the following behaviors:

```xml
<DataGrid ItemsSource="{Binding Items}">
  <i:Interaction.Behaviors>
    <dgx:BeginEditOnCtrlEnterBehavior/>
    <dgx:ExtendedStarSizeBehavior/>
    <dgx:DisableTargetWhileEditingBehavior Target="{Binding ElementName=ToolBar}"/>
  </i:Interaction.Behaviors>
```

### BeginEditOnCtrlEnterBehavior
Adds a new keyboard shortcut to start editing a cell without loosing the current content. 

When you have selected a column, starting to type will clear the content and replace it with the typed text. 

When the `BeginEditOnCtrlEnterBehavior` is enabled, you can click Ctrl+Enter to start editing the cell, preserving it's content.

### ExtendedStarSizeBehavior
Using star sizes as column width limits all columns to the width of the data grid. 

This behavior replaces the standard star size behavior with something more flexible: 

The columns will always use the available width of the data grid, but on resizing they only grow but don't shrink. 

This way it's also possible to manually grow the size of some columns. 

Pressing the Ctrl-Key while growing the width of a column will grow the width of all columns to the right, too. 

Pressing the Shift-Key while resizing a column or the whole gird will fit all columns into the grid again.

### DisableTargetWhileEditingBehavior
Disables a control on the page while the grid is in editing mode. 

Some elements that don't grab the focus can execute actions while the data grid is still in editing mode and cause the data grid to throw an exception. 

With this behavior it's easy to disable these controls, avoiding the problem.


