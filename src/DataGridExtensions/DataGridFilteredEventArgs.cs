namespace DataGridExtensions
{
    using System;

    /// <summary>
    /// Notification about filtering completed.
    /// </summary>
    public class DataGridFilteredEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataGridFilteredEventArgs"/> class.
        /// </summary>
        /// <param name="itemsCount">Number of elements which match the filter(s).</param>
        public DataGridFilteredEventArgs(int itemsCount)
        {
            MatchingItemsCount = itemsCount;
        }
        /// <summary>
        /// Gets the number of elements which match the filter(s).
        /// </summary>
        public int MatchingItemsCount { get; }
    }
}
