using System.ComponentModel;

namespace DataGridExtensions
{
    using System.Collections.Generic;
    using System.Windows.Controls;

    /// <summary>
    /// Interface to be implemented by view models that support custom filtering and sorting.
    /// </summary>
    public interface ICustomFilter
    {
        /// <summary>
        /// Called when the sort criteria has changed.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="sortDescriptions">The current collection of sort descriptions.</param>
        void OnSortChanged(DataGrid dataGrid, IReadOnlyCollection<SortDescription> sortDescriptions);

        /// <summary>
        /// Called when the filter criteria has changed.
        /// </summary>
        /// <param name="dataGrid"></param>
        /// <param name="dataGridColumns">The current collection of DataGrid columns with filter criteria.</param>
        void OnFilterChanged(DataGrid dataGrid, IReadOnlyCollection<DataGridColumn> dataGridColumns);
    }
}
