using LPR381Solver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    public enum BranchAndBoundFathomReason
    {
        Branched,           // internal node — spawned a Down and an Up child
        IntegerFeasible,    // LP relaxation is already integer where required — a leaf
        BoundNotPromising,  // bound can't beat current best — fathomed
        Infeasible,         // LP relaxation has no feasible solution — fathomed
        Unbounded           // LP relaxation is unbounded (only expected at the root)
    }

    public class BranchAndBoundNode
    {
        public int NodeId { get; set; }
        public int ParentNodeId { get; set; }        // -1 for root

        public LPModel Model { get; set; }             // this node's LP relaxation to solve
        public int BranchVariableIndex { get; set; }    // variable branched on to reach this node, -1 for root
        public string BranchDescription { get; set; }   // e.g. "x2 <= 3"

        public SolveResult Result { get; set; }          // this node's LP relaxation solve result
        public BranchAndBoundFathomReason Fathom { get; set; }
        public bool BecameNewIncumbent { get; set; }

        public BranchAndBoundNode(int nodeId, int parentNodeId, LPModel model)
        {
            NodeId = nodeId;
            ParentNodeId = parentNodeId;
            Model = model;
            BranchVariableIndex = -1;
            BranchDescription = "root subproblem (LP relaxation of the original model)";
        }
    }
}
