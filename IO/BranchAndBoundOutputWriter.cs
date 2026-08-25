using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPR381Solver.Core;
using LPR381Solver.Models;

namespace LPR381Solver.IO
{
    public static class BranchAndBoundOutputWriter
    {
        public static string BuildReport(LPModel model, BranchAndBoundSolveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("==============================================").Append("\n");
            sb.Append(" LPR381 Programming Solver - Output Report").Append("\n");
            sb.Append(" Algorithm: Branch & Bound Simplex Algorithm").Append("\n");
            sb.Append("==============================================").Append("\n\n");

            sb.Append("Original Model:").Append("\n");
            sb.Append(DescribeModel(model)).Append("\n");

            sb.Append("Branch & Bound Subproblem Trace:").Append("\n\n");

            for (int i = 0; i < result.ExploredNodes.Count; i++)
            {
                sb.Append(DescribeNode(result.ExploredNodes[i])).Append("\n");
            }

            sb.Append("Result:").Append("\n");
            if (result.Status == SolveStatus.Optimal)
            {
                sb.Append("Status: Optimal").Append("\n");
                sb.Append("Best candidate: Node ").Append(result.BestNode.NodeId).Append("\n");
                sb.Append("Objective value: ").Append(result.ObjectiveValue).Append("\n");
                for (int i = 0; i < result.OriginalVariableValues.Length; i++)
                {
                    sb.Append("x").Append(i + 1).Append(" = ")
                      .Append(System.Math.Round(result.OriginalVariableValues[i], 3)).Append("\n");
                }
            }
            else if (result.Status == SolveStatus.Infeasible)
            {
                sb.Append("Status: INFEASIBLE - no integer-feasible solution exists.").Append("\n");
            }
            else if (result.Status == SolveStatus.Unbounded)
            {
                sb.Append("Status: UNBOUNDED - the LP relaxation is unbounded.").Append("\n");
            }

            return sb.ToString();
        }

        public static void WriteToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        private static string DescribeNode(BranchAndBoundNode node)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("========================================").Append("\n");
            sb.Append("Node ").Append(node.NodeId);
            sb.Append(" (parent=").Append(node.ParentNodeId == -1 ? "root" : node.ParentNodeId.ToString()).Append(")");
            sb.Append(" - ").Append(node.BranchDescription).Append("\n");
            sb.Append("========================================").Append("\n");

            if (node.Result != null)
            {
                for (int i = 0; i < node.Result.Iterations.Count; i++)
                {
                    sb.Append(node.Result.Iterations[i].ToDisplayString(i)).Append("\n");
                }
            }

            sb.Append("Node status: ").Append(FathomText(node.Fathom));
            if (node.BecameNewIncumbent) sb.Append("  <-- new best candidate");
            sb.Append("\n");

            return sb.ToString();
        }

        private static string FathomText(BranchAndBoundFathomReason reason)
        {
            if (reason == BranchAndBoundFathomReason.Branched) return "Branched further (not fathomed)";
            if (reason == BranchAndBoundFathomReason.IntegerFeasible) return "Fathomed - integer-feasible candidate";
            if (reason == BranchAndBoundFathomReason.BoundNotPromising) return "Fathomed - bound cannot beat current best";
            if (reason == BranchAndBoundFathomReason.Infeasible) return "Fathomed - LP relaxation infeasible";
            if (reason == BranchAndBoundFathomReason.Unbounded) return "Fathomed - LP relaxation unbounded";
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
    }
}
