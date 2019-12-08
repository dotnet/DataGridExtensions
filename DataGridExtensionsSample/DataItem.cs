using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridExtensionsSample
{
    using System.Windows;

    public class DataItem
    {
        private static readonly Random _rand = new Random();
        private static readonly string[] _samples = new[] {"lorem", "ipsum", "dolor", "sit", "amet"};

        public DataItem(int index)
        {
            Flag = _rand.Next(2) == 0;
            Index = index;
            Column1 = Guid.NewGuid().ToString();
            Column2 = _rand.Next(20) == 0 ? null : Guid.NewGuid().ToString();
            Column3 = Guid.NewGuid().ToString();
            Column4 = Guid.NewGuid().ToString();
            Column5 = _samples[_rand.Next(_samples.Length)];
            Column6 = (Visibility)_rand.Next(2);
            Probability = _rand.NextDouble();
        }

        public bool Flag { get; private set; }
        public int Index { get; private set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
        public string Column3 { get; set; }
        public string Column4 { get; set; }
        public string Column5 { get; set; }
        public Visibility Column6 { get; set; }
        public double Probability { get; private set; }
    }
}
