using System;
using System.Collections.Generic;
using System.Text;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    public class KnapsackSolveResult
    {
        public SolveStatus Status { get; set; }

        public List<KnapsackItem> Items { get; set; }
        public List<KnapsackNode> ExploredNodes { get; set; }
        public KnapsackNode BestNode { get; set; }

        public double[] OriginalVariableValues { get; set; }
        public double ObjectiveValue { get; set; }
        public double Capacity { get; set; }

        public KnapsackSolveResult()
        {
            Items = new List<KnapsackItem>();
            ExploredNodes = new List<KnapsackNode>();
        }
    }
}
