namespace DataGridExtensions
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// Provides additional events for the <see cref="DataGrid"/>
    /// </summary>
    public interface IDataGridEventsProvider
    {
        /// <summary>
        /// Occurs when the visibility of a column has changed.
        /// </summary>
        event EventHandler<DataGridColumnEventArgs>? ColumnVisibilityChanged;

        /// <summary>
        /// Occurs when the actual width of a column has changed.
        /// </summary>
        event EventHandler<DataGridColumnEventArgs>? ColumnActualWidthChanged;

        /// <summary>
        /// Occurs when the display index of a column has changed.
        /// </summary>
        event EventHandler<DataGridColumnEventArgs>? ColumnDisplayIndexChanged;

        /// <summary>
        /// Occurs when the sort direction of a column has changed.
        /// </summary>
        event EventHandler<DataGridColumnEventArgs>? ColumnSortDirectionChanged;
    }
}
