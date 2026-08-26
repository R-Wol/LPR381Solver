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
        
        // Builds the report for the Revised Primal Simplex Algorithm: every iteration's
        // Product Form (B^-1) and Price Out (y, Cj-Zj) calculation, followed by the result.
        public static string BuildRevisedReport(LPModel model, string algorithmName, RevisedSolveResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("==============================================").Append("\n");
            sb.Append(" LPR381 Programming Solver - Output Report").Append("\n");
            sb.Append(" Algorithm: ").Append(algorithmName).Append("\n");
            sb.Append("==============================================").Append("\n\n");

            sb.Append("Original Model:").Append("\n");
            sb.Append(DescribeModel(model)).Append("\n");

            sb.Append("Canonical Form: (the starting basis below is Iteration 0's Basis / B^-1 = Identity)").Append("\n\n");

            string[] colNames = result.ColumnNames;
            for (int idx = 0; idx < result.Iterations.Count; idx++)
            {
                RevisedIteration it = result.Iterations[idx];
                sb.Append("--- Iteration ").Append(it.IterationNumber).Append(" ---").Append("\n");

                sb.Append("Basis: ");
                for (int i = 0; i < it.Basis.Length; i++)
                {
                    sb.Append(colNames[it.Basis[i]]).Append(" ");
                }
                sb.Append("\n");

                sb.Append("B^-1 (Product Form of the Inverse):").Append("\n");
                sb.Append(FormatMatrix(it.BasisInverse)).Append("\n");

                sb.Append("xB = B^-1 . b: ").Append(FormatVector(it.XB)).Append("\n");
                sb.Append("y = cB^T . B^-1 (Price Out vector): ").Append(FormatVector(it.Y)).Append("\n");

                sb.Append("Cj - Zj row:").Append("\n");
                for (int j = 0; j < it.CjMinusZj.Length; j++)
                {
                    sb.Append("  ").Append(colNames[j]).Append(" = ").Append(Round3(it.CjMinusZj[j]));
                }
                sb.Append("\n");

                if (it.EnteringColumn == -1)
                {
                    sb.Append("Optimal - no entering column improves the objective.").Append("\n\n");
                }
                else if (it.LeavingRow == -1)
                {
                    // Entering column chosen, but the ratio test found no valid leaving row:
                    // every entry of B^-1 . Aj is <= 0, so this column can increase without limit.
                    sb.Append("Entering column: ").Append(colNames[it.EnteringColumn])
                      .Append(" (Cj-Zj = ").Append(Round3(it.CjMinusZj[it.EnteringColumn])).Append(")").Append("\n");
                    sb.Append("Entering column in basis coordinates (B^-1 . Aj): ")
                      .Append(FormatVector(it.EnteringColumnBasisCoords)).Append("\n");
                    sb.Append("No positive entry available for the ratio test - the objective is UNBOUNDED.").Append("\n\n");
                }
                else
                {
                    sb.Append("Entering column: ").Append(colNames[it.EnteringColumn])
                      .Append(" (Cj-Zj = ").Append(Round3(it.CjMinusZj[it.EnteringColumn])).Append(")").Append("\n");
                    sb.Append("Entering column in basis coordinates (B^-1 . Aj): ")
                      .Append(FormatVector(it.EnteringColumnBasisCoords)).Append("\n");
                    sb.Append("Leaving row: ").Append(it.LeavingRow)
                      .Append(" (basic variable ").Append(colNames[it.Basis[it.LeavingRow]]).Append(")").Append("\n");
                    sb.Append("Pivot value: ").Append(Round3(it.PivotValue)).Append("\n");
                    sb.Append("Eta column (applied to B^-1 for the next iteration): ")
                      .Append(FormatVector(it.EtaColumn)).Append("\n\n");
                }
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
        
        public static string BuildCuttingPlaneReport(LPModel model, string algorithmName, CuttingPlaneResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("==============================================").Append("\n");
            sb.Append(" LPR381 Programming Solver - Output Report").Append("\n");
            sb.Append(" Algorithm: ").Append(algorithmName).Append("\n");
            sb.Append("==============================================").Append("\n\n");

            sb.Append("Original Model:").Append("\n");
            sb.Append(DescribeModel(model)).Append("\n");

            for (int idx = 0; idx < result.Rounds.Count; idx++)
            {
                CuttingPlaneRound round = result.Rounds[idx];
                sb.Append("############################################").Append("\n");
                sb.Append("# Cut Round ").Append(round.RoundNumber)
                  .Append(round.RoundNumber == 0 ? " (original Canonical Form, no cuts yet)" : " (with all cuts added so far)")
                  .Append("\n");
                sb.Append("############################################").Append("\n\n");

                string[] colNames = round.Relaxation.ColumnNames;
                for (int i = 0; i < round.Relaxation.Iterations.Count; i++)
                {
                    RevisedIteration it = round.Relaxation.Iterations[i];
                    sb.Append("--- Iteration ").Append(it.IterationNumber).Append(" ---").Append("\n");
                    sb.Append("Basis: ");
                    for (int b = 0; b < it.Basis.Length; b++)
                    {
                        sb.Append(colNames[it.Basis[b]]).Append(" ");
                    }
                    sb.Append("\n");
                    sb.Append("B^-1 (Product Form of the Inverse):").Append("\n");
                    sb.Append(FormatMatrix(it.BasisInverse)).Append("\n");
                    sb.Append("xB = B^-1 . b: ").Append(FormatVector(it.XB)).Append("\n");
                    sb.Append("y = cB^T . B^-1 (Price Out vector): ").Append(FormatVector(it.Y)).Append("\n");
                    if (it.EnteringColumn == -1)
                    {
                        sb.Append("Optimal for this round - no entering column improves the objective.").Append("\n\n");
                    }
                    else
                    {
                        sb.Append("Entering column: ").Append(colNames[it.EnteringColumn]).Append("\n\n");
                    }
                }

                if (round.Relaxation.Status != SolveStatus.Optimal)
                {
                    sb.Append("This round's relaxation is ").Append(round.Relaxation.Status).Append(".").Append("\n\n");
                    continue;
                }

                sb.Append("Relaxation objective: ").Append(Round3(round.Relaxation.ObjectiveValue)).Append("\n");
                sb.Append("Relaxation solution: ");
                for (int i = 0; i < round.Relaxation.OriginalVariableValues.Length; i++)
                {
                    sb.Append("x").Append(i + 1).Append("=").Append(Round3(round.Relaxation.OriginalVariableValues[i])).Append(" ");
                }
                sb.Append("\n");

                if (round.IsIntegerFeasible)
                {
                    sb.Append("-> INTEGER FEASIBLE. Stopping - this is the optimal solution.").Append("\n\n");
                }
                else
                {
                    sb.Append("-> x").Append(round.FractionalVariableIndex + 1).Append(" is fractional. Deriving a Gomory cut").Append("\n");
                    sb.Append("New cut (in working-variable space): ").Append(FormatVector(round.CutCoefficients))
                      .Append(" . w >= ").Append(Round3(round.CutRhs)).Append("\n\n");
                }
            }

            sb.Append("Result:").Append("\n");
            if (result.CutLimitReached)
            {
                sb.Append("Status: DID NOT CONVERGE within the cut limit for this build. ")
                  .Append("This is a safety cap, not expected for problems of this project's scale.").Append("\n");
            }
            else if (result.Status == SolveStatus.Optimal)
            {
                sb.Append("Status: Optimal").Append("\n");
                sb.Append("Objective value: ").Append(Round3(result.ObjectiveValue)).Append("\n");
                for (int i = 0; i < result.OriginalVariableValues.Length; i++)
                {
                    sb.Append("x").Append(i + 1).Append(" = ").Append(Round3(result.OriginalVariableValues[i])).Append("\n");
                }
            }
            else if (result.Status == SolveStatus.Infeasible)
            {
                sb.Append("Status: INFEASIBLE - no integer-feasible solution exists.").Append("\n");
            }
            else if (result.Status == SolveStatus.Unbounded)
            {
                sb.Append("Status: UNBOUNDED - the objective can be improved without limit.").Append("\n");
            }

            return sb.ToString();
        }

        private static string FormatVector(double[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[ ");
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append(Round3(values[i])).Append(" ");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private static string FormatMatrix(double[,] matrix)
        {
            StringBuilder sb = new StringBuilder();
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                sb.Append("  [ ");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(Round3(matrix[i, j])).Append(" ");
                }
                sb.Append("]").Append("\n");
            }
            return sb.ToString();
        }

        private static double Round3(double value)
        {
            return System.Math.Round(value, 3);
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
