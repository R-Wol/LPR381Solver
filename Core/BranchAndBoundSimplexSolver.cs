using LPR381Solver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    public static class BranchAndBoundSimplexSolver
    {
        private const double Tolerance = 1e-6;

        public static BranchAndBoundSolveResult Solve(LPModel model)
        {
            List<int> intVarIndices = new List<int>();
            for (int i = 0; i < model.VariableCount; i++)
            {
                if (model.SignRestrictions[i] == SignRestriction.Int || model.SignRestrictions[i] == SignRestriction.Bin)
                {
                    intVarIndices.Add(i);
                }
            }

            BranchAndBoundSolveResult result = new BranchAndBoundSolveResult();
            bool isMaximize = model.ObjectiveType == ObjectiveType.Maximize;

            int nextNodeId = 0;
            BranchAndBoundNode root = new BranchAndBoundNode(nextNodeId, -1, model);
            nextNodeId++;

            Stack<BranchAndBoundNode> stack = new Stack<BranchAndBoundNode>();
            stack.Push(root);

            BranchAndBoundNode best = null;
            double bestValue = isMaximize ? double.NegativeInfinity : double.PositiveInfinity;
            bool rootUnbounded = false;

            while (stack.Count > 0)
            {
                BranchAndBoundNode node = stack.Pop();
                node.Result = PrimalSimplexSolver.Solve(node.Model);

                if (node.Result.Status == SolveStatus.Infeasible)
                {
                    node.Fathom = BranchAndBoundFathomReason.Infeasible;
                    result.ExploredNodes.Add(node);
                    continue;
                }

                if (node.Result.Status == SolveStatus.Unbounded)
                {
                    node.Fathom = BranchAndBoundFathomReason.Unbounded;
                    result.ExploredNodes.Add(node);
                    if (node.ParentNodeId == -1) rootUnbounded = true;
                    continue;
                }

                double bound = node.Result.ObjectiveValue;
                bool cannotImprove = isMaximize ? bound <= bestValue + Tolerance : bound >= bestValue - Tolerance;
                if (best != null && cannotImprove)
                {
                    node.Fathom = BranchAndBoundFathomReason.BoundNotPromising;
                    result.ExploredNodes.Add(node);
                    continue;
                }

                int fractionalIndex = FindFirstFractionalIndex(node.Result.OriginalVariableValues, intVarIndices);

                if (fractionalIndex == -1)
                {
                    node.Fathom = BranchAndBoundFathomReason.IntegerFeasible;
                    bool isBetter = best == null || (isMaximize ? bound > bestValue + Tolerance : bound < bestValue - Tolerance);
                    if (isBetter)
                    {
                        bestValue = bound;
                        best = node;
                        node.BecameNewIncumbent = true;
                    }
                    result.ExploredNodes.Add(node);
                    continue;
                }

                node.Fathom = BranchAndBoundFathomReason.Branched;
                result.ExploredNodes.Add(node);

                double value = node.Result.OriginalVariableValues[fractionalIndex];
                double floorValue = Math.Floor(value);
                double ceilValue = Math.Ceiling(value);
                string varName = "x" + (fractionalIndex + 1);

                LPModel downModel = CloneModelWithExtraConstraint(node.Model, fractionalIndex, RelationType.LessOrEqual, floorValue);
                BranchAndBoundNode downChild = new BranchAndBoundNode(nextNodeId, node.NodeId, downModel);
                nextNodeId++;
                downChild.BranchVariableIndex = fractionalIndex;
                downChild.BranchDescription = varName + " <= " + floorValue;

                LPModel upModel = CloneModelWithExtraConstraint(node.Model, fractionalIndex, RelationType.GreaterOrEqual, ceilValue);
                BranchAndBoundNode upChild = new BranchAndBoundNode(nextNodeId, node.NodeId, upModel);
                nextNodeId++;
                upChild.BranchVariableIndex = fractionalIndex;
                upChild.BranchDescription = varName + " >= " + ceilValue;

                stack.Push(upChild);
                stack.Push(downChild); // pushed last -> popped first (round-down explored first)
            }

            result.BestNode = best;
            if (best != null)
            {
                result.Status = SolveStatus.Optimal;
                result.ObjectiveValue = best.Result.ObjectiveValue;
                result.OriginalVariableValues = best.Result.OriginalVariableValues;
            }
            else
            {
                result.Status = rootUnbounded ? SolveStatus.Unbounded : SolveStatus.Infeasible;
            }

            return result;
        }

        private static int FindFirstFractionalIndex(double[] values, List<int> indices)
        {
            for (int k = 0; k < indices.Count; k++)
            {
                int idx = indices[k];
                double v = values[idx];
                double rounded = Math.Round(v);
                if (Math.Abs(v - rounded) > Tolerance)
                {
                    return idx;
                }
            }
            return -1;
        }

        private static LPModel CloneModelWithExtraConstraint(LPModel source, int variableIndex, RelationType relation, double rhs)
        {
            LPModel clone = new LPModel();
            clone.ObjectiveType = source.ObjectiveType;
            clone.ObjectiveCoefficients = new List<double>(source.ObjectiveCoefficients);
            clone.SignRestrictions = new List<SignRestriction>(source.SignRestrictions);

            clone.Constraints = new List<Constraint>();
            for (int i = 0; i < source.Constraints.Count; i++)
            {
                clone.Constraints.Add(source.Constraints[i].Clone());
            }

            double[] boundRow = new double[source.VariableCount];
            boundRow[variableIndex] = 1.0;
            clone.Constraints.Add(new Constraint(new List<double>(boundRow), relation, rhs));

            return clone;
        }
    }
}
