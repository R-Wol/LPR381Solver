using System;
using System.Collections.Generic;
using System.Text;

namespace LPR381Solver.Core
{
    public class KnapsackItem
    {
        public int OriginalIndex { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public double Weight { get; set; }

        public double Ratio
        {
            get { return Weight == 0 ? double.PositiveInfinity : Value / Weight; }
        }

        public KnapsackItem(int originalIndex, string name, double value, double weight)
        {
            OriginalIndex = originalIndex;
            Name = name;
            Value = value;
            Weight = weight;
        }
    }
}
