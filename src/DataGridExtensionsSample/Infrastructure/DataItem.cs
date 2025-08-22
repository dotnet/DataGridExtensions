#pragma warning disable CA5394 // Do not use insecure randomness

namespace DataGridExtensionsSample.Infrastructure
{
    using System;
    using System.Windows;

    public class DataItem(int index)
    {
        private static readonly Random _rand = new();
        private static readonly string[] _samples = ["lorem", "ipsum", "dolor", "sit", "amet"];

        public bool Flag { get; } = _rand.Next(2) == 0;
        public int Index { get; } = index;
        public string? Column1 { get; set; } = Guid.NewGuid().ToString();
        public string? Column2 { get; set; } = _rand.Next(20) == 0 ? null : Guid.NewGuid().ToString();
        public string? Column3 { get; set; } = Guid.NewGuid().ToString();
        public string? Column4 { get; set; } = Guid.NewGuid().ToString();
        public string Column5 { get; set; } = _samples[_rand.Next(_samples.Length)];
        public Visibility Column6 { get; set; } = (Visibility)_rand.Next(3);
        public double Probability { get; } = _rand.NextDouble();
    }
}
