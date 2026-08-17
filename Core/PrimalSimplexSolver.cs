using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    // The Primal Simplex Algorithm (tableau method) with the Big-M technique for
    // handling '>=' and '=' constraints. Internally always maximises; a minimisation
    // problem has its objective row negated by StandardFormBuilder and the final
    // objective value is negated back below.
    public static class PrimalSimplexSolver
    {
        private const double Tolerance = 1e-9;
        private const int MaxIterations = 1000;

        public static SolveResult Solve(LPModel model)
        {
            WorkingModel workingModel = StandardFormBuilder.BuildWorkingModel(model);
            SimplexTableau tableau = StandardFormBuilder.BuildInitialTableau(workingModel);

            SolveResult result = new SolveResult();
            result.Iterations.Add(tableau.Clone()); // iteration 0 = the Canonical Form

            int iteration = 0;
            while (true)
            {
                double[] cz = tableau.ComputeCjMinusZj();

                // --- Optimality test: for a maximisation tableau we stop once no
                //     non-basic column can still improve the objective. ---
                int enteringColumn = -1;
                double best = Tolerance;
                for (int j = 0; j < tableau.ColumnCount; j++)
                {
                    if (IsBasic(tableau, j)) continue;
                    if (cz[j] > best)
                    {
                        best = cz[j];
                        enteringColumn = j;
                    }
                }

                if (enteringColumn == -1)
                {
                    break; // optimal tableau reached
                }

                // --- Ratio test: choose the leaving row. ---
                int leavingRow = -1;
                double bestRatio = 0.0;
                for (int i = 0; i < tableau.RowCount; i++)
                {
                    if (tableau.A[i, enteringColumn] > Tolerance)
                    {
                        double ratio = tableau.B[i] / tableau.A[i, enteringColumn];
                        if (leavingRow == -1 || ratio < bestRatio - 1e-12)
                        {
                            bestRatio = ratio;
                            leavingRow = i;
                        }
                    }
                }

                if (leavingRow == -1)
                {
                    result.Status = SolveStatus.Unbounded;
                    return result;
                }

                Pivot(tableau, leavingRow, enteringColumn);
                iteration = iteration + 1;
                result.Iterations.Add(tableau.Clone());

                if (iteration > MaxIterations)
                {
                    result.Status = SolveStatus.Unbounded; // safety net - should not normally happen
                    return result;
                }
            }

            // --- Feasibility check: if any artificial variable is still basic at a
            //     positive value, the original problem has no feasible solution. ---
            for (int i = 0; i < tableau.RowCount; i++)
            {
                string basicName = tableau.ColumnNames[tableau.Basis[i]];
                if (basicName.StartsWith("a") && tableau.B[i] > 1e-6)
                {
                    result.Status = SolveStatus.Infeasible;
                    return result;
                }
            }

            // --- Extract the working-variable solution, then map it back to the
            //     original decision variables (undoing any urs-split / negation). ---
            double[] workingValues = new double[workingModel.VariableCount];
            for (int i = 0; i < tableau.RowCount; i++)
            {
                if (tableau.Basis[i] < workingModel.VariableCount)
                {
                    workingValues[tableau.Basis[i]] = tableau.B[i];
                }
            }

            double[] originalValues = new double[model.VariableCount];
            for (int i = 0; i < model.VariableCount; i++)
            {
                originalValues[i] = workingModel.OriginalVariableMappings[i].ResolveValue(workingValues);
            }

            double internalObjective = tableau.ComputeObjectiveValue();
            double finalObjective = workingModel.IsMaximize ? internalObjective : -internalObjective;

            result.Status = SolveStatus.Optimal;
            result.OriginalVariableValues = originalValues;
            result.ObjectiveValue = System.Math.Round(finalObjective, 3);
            return result;
        }

        private static bool IsBasic(SimplexTableau tableau, int column)
        {
            for (int i = 0; i < tableau.RowCount; i++)
            {
                if (tableau.Basis[i] == column) return true;
            }
            return false;
        }

        private static void Pivot(SimplexTableau tableau, int pivotRow, int pivotColumn)
        {
            double pivotValue = tableau.A[pivotRow, pivotColumn];

            for (int j = 0; j < tableau.ColumnCount; j++)
            {
                tableau.A[pivotRow, j] = tableau.A[pivotRow, j] / pivotValue;
            }
            tableau.B[pivotRow] = tableau.B[pivotRow] / pivotValue;

            for (int i = 0; i < tableau.RowCount; i++)
            {
                if (i == pivotRow) continue;
                double factor = tableau.A[i, pivotColumn];
                if (System.Math.Abs(factor) < 1e-12) continue;

                for (int j = 0; j < tableau.ColumnCount; j++)
                {
                    tableau.A[i, j] = tableau.A[i, j] - factor * tableau.A[pivotRow, j];
                }
                tableau.B[i] = tableau.B[i] - factor * tableau.B[pivotRow];
            }

            tableau.Basis[pivotRow] = pivotColumn;
        }
    }
}
