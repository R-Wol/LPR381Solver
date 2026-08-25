using System;
using System.Collections.Generic;
using System.Text;

namespace LPR381Solver.Core
{
    public enum FathomReason
    {
        Branched,
        IntegerCandidate,
        BoundNotPromising,
        CapacityExceeded
    }

    public class KnapsackNode
    {
        public int NodeId { get; set; }
        public int ParentNodeId { get; set; }
        public int BranchedItemIndex { get; set; }
        public bool? BranchDecision { get; set; }
        public int NextItemIndex { get; set; }

        public bool?[] Decisions { get; set; }

        public double Value { get; set; }
        public double Weight { get; set; }
        public double Bound { get; set; }

        public FathomReason Fathom { get; set; }
        public bool BecameNewIncumbent { get; set; }

        public KnapsackNode(int nodeId, int parentNodeId, int itemCount)
        {
            NodeId = nodeId;
            ParentNodeId = parentNodeId;
            Decisions = new bool?[itemCount];
        }

        public KnapsackNode Clone(int newNodeId)
        {
            KnapsackNode copy = new KnapsackNode(newNodeId, NodeId, Decisions.Length);
            for (int i = 0; i < Decisions.Length; i++) copy.Decisions[i] = Decisions[i];
            copy.Value = Value;
            copy.Weight = Weight;
            return copy;
        }
    }

}
