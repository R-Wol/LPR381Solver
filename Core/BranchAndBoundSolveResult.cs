using LPR381Solver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    public class BranchAndBoundSolveResult
    {
        public SolveStatus Status { get; set; }
        public List<BranchAndBoundNode> ExploredNodes { get; set; }
        public BranchAndBoundNode BestNode { get; set; }

        public double[] OriginalVariableValues { get; set; }
        public double ObjectiveValue { get; set; }

        public BranchAndBoundSolveResult()
        {
            ExploredNodes = new List<BranchAndBoundNode>();
        }
    }
}
