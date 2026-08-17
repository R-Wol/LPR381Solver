using System.IO;
using System.Text;
using LPR381Solver.Core;
using LPR381Solver.Models;

namespace LPR381Solver.IO
{
    // Writes the Canonical Form, every tableau iteration, and the final solution
    // of a solved model to a plain text output file (and returns the same text so
    // it can also be echoed to the console).
    public static class OutputWriter
    {
        public static string BuildReport(LPModel model, string algorithmName, SolveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("==============================================").Append("\n");
            sb.Append(" LPR381 Programming Solver - Output Report").Append("\n");
            sb.Append(" Algorithm: ").Append(algorithmName).Append("\n");
            sb.Append("==============================================").Append("\n\n");

            sb.Append("Original Model:").Append("\n");
            sb.Append(DescribeModel(model)).Append("\n");

            sb.Append("Canonical Form and Tableau Iterations:").Append("\n");
            sb.Append("(Iteration 0 is the Canonical Form before any pivoting.)").Append("\n\n");

            for (int i = 0; i < result.Iterations.Count; i++)
            {
                sb.Append(result.Iterations[i].ToDisplayString(i)).Append("\n");
            }

            sb.Append("Result:").Append("\n");
            if (result.Status == SolveStatus.Optimal)
            {
                sb.Append("Status: Optimal").Append("\n");
                sb.Append("Objective value: ").Append(result.ObjectiveValue).Append("\n");
                for (int i = 0; i < result.OriginalVariableValues.Length; i++)
                {
                    sb.Append("x").Append(i + 1).Append(" = ")
                      .Append(System.Math.Round(result.OriginalVariableValues[i], 3)).Append("\n");
                }
            }
            else if (result.Status == SolveStatus.Infeasible)
            {
                sb.Append("Status: INFEASIBLE - the model has no feasible solution.").Append("\n");
            }
            else if (result.Status == SolveStatus.Unbounded)
            {
                sb.Append("Status: UNBOUNDED - the objective can be improved without limit.").Append("\n");
            }

            return sb.ToString();
        }

        public static void WriteToFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
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
