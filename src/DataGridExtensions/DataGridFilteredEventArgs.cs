namespace DataGridExtensions
{
    using System;
    using System.Collections;

    /// <summary>
    /// Notification about filtering completed.
    /// </summary>
    public class DataGridFilteredEventArgs : EventArgs
    {
        /// <summary>
        /// Elements which match the filter(s).
        /// </summary>
        public IList MatchingItems { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFilteredEventArgs"/> class.
        /// </summary>
        /// <param name="itemsList"><see cref="IList"/> of element which match the filter(s).
        public DataGridFilteredEventArgs(IList itemsList)
        {
            MatchingItems = itemsList;
        }
    }
}
