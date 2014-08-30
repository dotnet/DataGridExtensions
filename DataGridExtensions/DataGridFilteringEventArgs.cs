using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;

namespace DataGridExtensions
{
    /// <summary>
    /// Notification about additional columns to be filtered.
    /// Clients can e.g. use this event to cache/preload column data in a different thread and/or display a wait cursor while filtering.
    /// <remarks>
    /// Clients may only cancel the processing when e.g. the data grid is about to be unloaded. Canceling the process of filtering 
    /// will cause the UI to be inconsistent.
    /// </remarks>
    /// </summary>
    public class DataGridFilteringEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFilteringEventArgs"/> class.
        /// </summary>
        /// <param name="columns">The additional columns that will be filtered.</param>
        public DataGridFilteringEventArgs(ICollection<DataGridColumn> columns)
        {
            Columns = columns;
        }

        /// <summary>
        /// Gets the additional columns that will be filtered.
        /// </summary>
        public ICollection<DataGridColumn> Columns
        {
            get;
            private set;
        }
    }
}
