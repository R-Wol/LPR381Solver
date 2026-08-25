using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Core;
using LPR381Solver.Models;

namespace LPR381Solver.IO
{
    public static class KnapsackOutputWriter
    {
        public static string BuildReport(LPModel model, KnapsackSolveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("==============================================").Append("\n");
            sb.Append(" LPR381 Programming Solver - Output Report").Append("\n");
            sb.Append(" Algorithm: Branch & Bound Knapsack Algorithm").Append("\n");
            sb.Append("==============================================").Append("\n\n");

            sb.Append("Original Model:").Append("\n");
            sb.Append(DescribeModel(model)).Append("\n");

            sb.Append("Items ranked by value/weight ratio (used for bounding):").Append("\n");
            sb.Append("Item\tValue\tWeight\tRatio").Append("\n");
            for (int i = 0; i < result.Items.Count; i++)
            {
                KnapsackItem item = result.Items[i];
                sb.Append(item.Name).Append("\t").Append(Round3(item.Value)).Append("\t")
                  .Append(Round3(item.Weight)).Append("\t").Append(Round3(item.Ratio)).Append("\n");
            }
            sb.Append("Capacity: ").Append(Round3(result.Capacity)).Append("\n\n");

            sb.Append("Branch & Bound Subproblem Trace:").Append("\n\n");

            for (int i = 0; i < result.ExploredNodes.Count; i++)
            {
                sb.Append(DescribeNode(result.ExploredNodes[i], result)).Append("\n");
            }

            sb.Append("Result:").Append("\n");
            if (result.BestNode == null)
            {
                sb.Append("Status: No feasible candidate found.").Append("\n");
            }
            else
            {
                sb.Append("Status: Optimal").Append("\n");
                sb.Append("Best candidate: Node ").Append(result.BestNode.NodeId).Append("\n");
                sb.Append("Objective value: ").Append(result.ObjectiveValue).Append("\n");
                for (int i = 0; i < result.OriginalVariableValues.Length; i++)
                {
                    sb.Append("x").Append(i + 1).Append(" = ").Append(result.OriginalVariableValues[i]).Append("\n");
                }
            }

            return sb.ToString();
        }

        public static void WriteToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        private static string DescribeNode(KnapsackNode node, KnapsackSolveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Node ").Append(node.NodeId);
            sb.Append(" (parent=").Append(node.ParentNodeId == -1 ? "root" : node.ParentNodeId.ToString()).Append(")");

            if (node.BranchedItemIndex >= 0)
            {
                string itemName = result.Items[node.BranchedItemIndex].Name;
                string decision = node.BranchDecision == true ? "Include" : "Exclude";
                sb.Append(" - branched on ").Append(itemName).Append(": ").Append(decision);
            }
            else
            {
                sb.Append(" - root subproblem");
            }
            sb.Append("\n");

            sb.Append("  Value: ").Append(Round3(node.Value));
            sb.Append("  Weight: ").Append(Round3(node.Weight));
            sb.Append("  Bound: ").Append(Round3(node.Bound)).Append("\n");

            sb.Append("  Status: ").Append(FathomText(node.Fathom));
            if (node.BecameNewIncumbent) sb.Append("  <-- new best candidate");
            sb.Append("\n");

            return sb.ToString();
        }

        private static string FathomText(FathomReason reason)
        {
            if (reason == FathomReason.Branched) return "Branched further";
            if (reason == FathomReason.IntegerCandidate) return "Fathomed - complete integer solution";
            if (reason == FathomReason.BoundNotPromising) return "Fathomed - bound cannot beat current best";
            if (reason == FathomReason.CapacityExceeded) return "Fathomed - exceeds capacity";
            return reason.ToString();
        }

        private static string DescribeModel(LPModel model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(model.ObjectiveType == ObjectiveType.Maximize ? "max " : "min ");
            for (int i = 0; i < model.ObjectiveCoefficients.Count; i++)
            {
                sb.Append(FormatSigned(model.ObjectiveCoefficients[i])).Append(" x").Append(i + 1).Append(" ");
            }
            sb.Append("\n");

            for (int c = 0; c < model.Constraints.Count; c++)
            {
                Constraint con = model.Constraints[c];
                for (int i = 0; i < con.Coefficients.Count; i++)
                {
                    sb.Append(FormatSigned(con.Coefficients[i])).Append(" x").Append(i + 1).Append(" ");
                }
                sb.Append(RelationSymbol(con.Relation)).Append(" ").Append(con.Rhs).Append("\n");
            }

            sb.Append("Sign restrictions: ");
            for (int i = 0; i < model.SignRestrictions.Count; i++)
            {
                sb.Append(model.SignRestrictions[i]).Append(" ");
            }
            sb.Append("\n");

            return sb.ToString();
        }

        private static string FormatSigned(double value)
        {
            if (value >= 0) return "+" + value;
            return value.ToString();
        }

        private static string RelationSymbol(RelationType relation)
        {
            if (relation == RelationType.LessOrEqual) return "<=";
            if (relation == RelationType.GreaterOrEqual) return ">=";
            return "=";
        }

        private static double Round3(double value)
        {
            return System.Math.Round(value, 3);
        }
    }
}
