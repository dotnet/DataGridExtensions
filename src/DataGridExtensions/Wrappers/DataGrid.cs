namespace DataGridExtensions.Wrappers;

using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

/// <summary>
/// A custom DataGrid that overrides the OnCreateAutomationPeer method to return a custom automation peer to fix
/// https://github.com/dotnet/wpf/issues/5428
/// </summary>
public class DataGrid : System.Windows.Controls.DataGrid
{
    /// <inheritdoc cref="System.Windows.Controls.DataGrid.OnCreateAutomationPeer"/>
    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return new DataGridAutomationPeerWrapper(this);
    }

    private sealed class DataGridAutomationPeerWrapper(DataGrid owner)
        : ItemsControlAutomationPeer(owner), ISelectionProvider, ITableProvider
    {
        private readonly DataGridAutomationPeer _dataGridPeer = new(owner);

        bool ISelectionProvider.CanSelectMultiple => ((ISelectionProvider)_dataGridPeer).CanSelectMultiple;

        bool ISelectionProvider.IsSelectionRequired => ((ISelectionProvider)_dataGridPeer).IsSelectionRequired;

        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            return ((ISelectionProvider)_dataGridPeer).GetSelection();
        }

        int IGridProvider.RowCount => ((IGridProvider)_dataGridPeer).RowCount;

        int IGridProvider.ColumnCount => ((IGridProvider)_dataGridPeer).ColumnCount;

        RowOrColumnMajor ITableProvider.RowOrColumnMajor => ((ITableProvider)_dataGridPeer).RowOrColumnMajor;

        IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
        {
            return ((IGridProvider)_dataGridPeer).GetItem(row, column);
        }

        IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
        {
            return ((ITableProvider)_dataGridPeer).GetRowHeaders();
        }

        IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
        {
            return ((ITableProvider)_dataGridPeer).GetColumnHeaders();
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.DataGrid;
        }

        protected override string GetClassNameCore()
        {
            return Owner.GetType().Name;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            return _dataGridPeer.GetPattern(patternInterface);
        }

        protected override ItemAutomationPeer? CreateItemAutomationPeer(object? item)
        {
            return item == null ? null : new DataGridItemAutomationPeer(item, _dataGridPeer);
        }

        public static implicit operator DataGridAutomationPeer(DataGridAutomationPeerWrapper wrapper)
        {
            return wrapper._dataGridPeer;
        }
    }
}
