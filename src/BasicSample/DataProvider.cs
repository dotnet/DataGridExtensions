namespace BasicSample
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public static class DataProvider
    {
        /// <summary>
        /// Provide a simple list of 100 random items.
        /// </summary>
        public static IList<DataItem> Items { get; } = CreateItems();

        private static ObservableCollection<DataItem> CreateItems()
        {
            return new ObservableCollection<DataItem>(Enumerable.Range(0, 100).Select(index => new DataItem(index)));
        }
    }
}
