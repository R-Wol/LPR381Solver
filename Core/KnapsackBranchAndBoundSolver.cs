using LPR381Solver.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static LPR381Solver.Core.KnapsackNode;

namespace LPR381Solver.Core
{
    internal class KnapsackBranchAndBoundSolver
    {
        private const double Tolerance = 1e-9;

        public static KnapsackSolveResult Solve(LPModel model)
        {
            ValidateIsKnapsackModel(model);

            KnapsackSolveResult result = new KnapsackSolveResult();
            Constraint constraint = model.Constraints[0];
            double capacity = constraint.Rhs;
            result.Capacity = capacity;

            List<KnapsackItem> items = new List<KnapsackItem>();
            for (int i = 0; i < model.VariableCount; i++)
            {
                items.Add(new KnapsackItem(i, "x" + (i + 1), model.ObjectiveCoefficients[i], constraint.Coefficients[i]));
            }
            items.Sort(delegate (KnapsackItem a, KnapsackItem b) { return b.Ratio.CompareTo(a.Ratio); });
            result.Items = items;

            int n = items.Count;
            int nextNodeId = 0;

            KnapsackNode root = new KnapsackNode(nextNodeId, -1, n);
            nextNodeId++;
            root.BranchedItemIndex = -1;
            root.NextItemIndex = 0;
            root.Bound = ComputeBound(0.0, 0.0, 0, items, capacity);

            Stack<KnapsackNode> stack = new Stack<KnapsackNode>();
            stack.Push(root);

            KnapsackNode best = null;
            double bestValue = -1.0; // "exclude everything" (value 0) is always feasible

            while (stack.Count > 0)
            {
                KnapsackNode node = stack.Pop();

                if (node.NextItemIndex < n && node.Bound <= bestValue + Tolerance)
                {
                    node.Fathom = FathomReason.BoundNotPromising;
                    result.ExploredNodes.Add(node);
                    continue;
                }

                if (node.NextItemIndex >= n)
                {
                    node.Fathom = FathomReason.IntegerCandidate;
                    if (node.Value > bestValue + Tolerance)
                    {
                        bestValue = node.Value;
                        best = node;
                        node.BecameNewIncumbent = true;
                    }
                    result.ExploredNodes.Add(node);
                    continue;
                }

                node.Fathom = FathomReason.Branched;
                result.ExploredNodes.Add(node);

                int branchItem = node.NextItemIndex;
                KnapsackItem item = items[branchItem];

                KnapsackNode excludeChild = node.Clone(nextNodeId++);
                excludeChild.BranchedItemIndex = branchItem;
                excludeChild.BranchDecision = false;
                excludeChild.Decisions[branchItem] = false;
                excludeChild.NextItemIndex = branchItem + 1;
                excludeChild.Bound = ComputeBound(excludeChild.Value, excludeChild.Weight, excludeChild.NextItemIndex, items, capacity);

                KnapsackNode includeChild = node.Clone(nextNodeId++);
                includeChild.BranchedItemIndex = branchItem;
                includeChild.BranchDecision = true;
                includeChild.Decisions[branchItem] = true;
                includeChild.Value = node.Value + item.Value;
                includeChild.Weight = node.Weight + item.Weight;
                includeChild.NextItemIndex = branchItem + 1;

                if (includeChild.Weight > capacity + Tolerance)
                {
                    includeChild.Fathom = FathomReason.CapacityExceeded;
                    result.ExploredNodes.Add(includeChild);
                }
                else
                {
                    includeChild.Bound = ComputeBound(includeChild.Value, includeChild.Weight, includeChild.NextItemIndex, items, capacity);
                    if (includeChild.Bound <= bestValue + Tolerance)
                    {
                        includeChild.Fathom = FathomReason.BoundNotPromising;
                        result.ExploredNodes.Add(includeChild);
                    }
                    else
                    {
                        stack.Push(includeChild); // pushed last -> popped first (include branch explored greedily first)
                    }
                }

                if (excludeChild.Bound <= bestValue + Tolerance)
                {
                    excludeChild.Fathom = FathomReason.BoundNotPromising;
                    result.ExploredNodes.Add(excludeChild);
                }
                else
                {
                    stack.Push(excludeChild);
                }
            }

            result.Status = SolveStatus.Optimal;
            result.BestNode = best;
            result.ObjectiveValue = best == null ? 0.0 : Math.Round(bestValue, 3);

            double[] originalValues = new double[model.VariableCount];
            if (best != null)
            {
                for (int i = 0; i < n; i++)
                {
                    originalValues[items[i].OriginalIndex] = best.Decisions[i] == true ? 1.0 : 0.0;
                }
            }
            result.OriginalVariableValues = originalValues;

            return result;
        }

        private static double ComputeBound(double currentValue, double currentWeight, int startIndex, List<KnapsackItem> items, double capacity)
        {
            double remaining = capacity - currentWeight;
            double bound = currentValue;

            for (int i = startIndex; i < items.Count; i++)
            {
                if (remaining <= Tolerance) break;

                KnapsackItem item = items[i];
                if (item.Weight <= remaining)
                {
                    bound += item.Value;
                    remaining -= item.Weight;
                }
                else
                {
                    bound += item.Ratio * remaining;
                    remaining = 0.0;
                    break;
                }
            }

            return bound;
        }

        private static void ValidateIsKnapsackModel(LPModel model)
        {
            if (model.ObjectiveType != ObjectiveType.Maximize)
                throw new FormatException("Knapsack Branch & Bound requires a maximization objective.");
            if (model.Constraints.Count != 1)
                throw new FormatException("Knapsack Branch & Bound requires exactly one constraint (the knapsack capacity).");
            if (model.Constraints[0].Relation != RelationType.LessOrEqual)
                throw new FormatException("Knapsack Branch & Bound requires the single constraint to use '<='.");

            for (int i = 0; i < model.VariableCount; i++)
            {
                if (model.SignRestrictions[i] != SignRestriction.Bin)
                    throw new FormatException("Knapsack Branch & Bound requires every decision variable to be 'bin'.");
                if (model.ObjectiveCoefficients[i] < 0)
                    throw new FormatException("Knapsack Branch & Bound requires non-negative objective coefficients (item values).");
                if (model.Constraints[0].Coefficients[i] < 0)
                    throw new FormatException("Knapsack Branch & Bound requires non-negative constraint coefficients (item weights).");
            }
        }
    }
}

