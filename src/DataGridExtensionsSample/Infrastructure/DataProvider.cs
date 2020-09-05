namespace DataGridExtensionsSample.Infrastructure
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Composition;
    using System.Linq;

    using TomsToolbox.Wpf;

    [Export, Shared]
    public class DataProvider : ObservableObject
    {
        /// <summary>
        /// Provide a simple list of 100 random items.
        /// </summary>
        public IList<DataItem> Items { get; private set; } = CreateItems();

        public void Reload()
        {
            Items = CreateItems();
        }

        private static ObservableCollection<DataItem> CreateItems()
        {
            return new ObservableCollection<DataItem>(Enumerable.Range(0, 100).Select(index => new DataItem(index)));
        }
    }
}
