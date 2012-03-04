using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridExtensionsSample
{
    public class DataItem
    {
        public DataItem(Random rand, int index)
        {
            Flag = rand.Next(2) == 0;
            Index = index;
            Column1 = Guid.NewGuid().ToString();
            Column2 = Guid.NewGuid().ToString();
            Probability = rand.NextDouble();
        }

        public bool Flag { get; private set; }
        public int Index { get; private set; }
        public string Column1 { get; private set; }
        public string Column2 { get; private set; }
        public double Probability { get; private set; }
    }
}
