using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    // Step 1: LPModel -> WorkingModel (every variable made non-negative).
    // Step 2: WorkingModel -> initial Big-M simplex tableau (SimplexTableau, "iteration 0").
    public static class StandardFormBuilder
    {
        // Big-M penalty used to discourage artificial variables from remaining in the basis.
        public const double BigM = 1000000.0;

        public static WorkingModel BuildWorkingModel(LPModel model)
        {
            WorkingModel wm = new WorkingModel();
            wm.IsMaximize = model.ObjectiveType == ObjectiveType.Maximize;

            int n = model.VariableCount;
            List<int> binWorkingColumns = new List<int>();

            // --- Decide the working column layout for every original variable ---
            for (int i = 0; i < n; i++)
            {
                SignRestriction restriction = model.SignRestrictions[i];
                string originalName = "x" + (i + 1);

                if (restriction == SignRestriction.Negative)
                {
                    int col = wm.VariableCount;
                    wm.VariableNames.Add(originalName + "'"); // x = -x'
                    wm.ObjectiveCoefficients.Add(0.0);
                    wm.OriginalVariableMappings.Add(new VariableMapping(MappingKind.Negated, col, -1));
                }
                else if (restriction == SignRestriction.Urs)
                {
                    int colPlus = wm.VariableCount;
                    wm.VariableNames.Add(originalName + "+");
                    wm.ObjectiveCoefficients.Add(0.0);
                    int colMinus = wm.VariableCount;
                    wm.VariableNames.Add(originalName + "-");
                    wm.ObjectiveCoefficients.Add(0.0);
                    wm.OriginalVariableMappings.Add(new VariableMapping(MappingKind.SplitDifference, colPlus, colMinus));
                }
                else
                {
                    // Positive, Int, Bin all map straight through to one non-negative working column.
                    int col = wm.VariableCount;
                    wm.VariableNames.Add(originalName);
                    wm.ObjectiveCoefficients.Add(0.0);
                    wm.OriginalVariableMappings.Add(new VariableMapping(MappingKind.Identity, col, -1));

                    if (restriction == SignRestriction.Bin)
                    {
                        binWorkingColumns.Add(col);
                    }
                }
            }

            // --- Fill in the working objective coefficients using the mappings above ---
            for (int i = 0; i < n; i++)
            {
                double coeff = model.ObjectiveCoefficients[i];
                VariableMapping map = wm.OriginalVariableMappings[i];

                if (map.Kind == MappingKind.Identity)
                {
                    wm.ObjectiveCoefficients[map.WorkingColumnPlus] = coeff;
                }
                else if (map.Kind == MappingKind.Negated)
                {
                    wm.ObjectiveCoefficients[map.WorkingColumnPlus] = -coeff;
                }
                else // SplitDifference
                {
                    wm.ObjectiveCoefficients[map.WorkingColumnPlus] = coeff;
                    wm.ObjectiveCoefficients[map.WorkingColumnMinus] = -coeff;
                }
            }

            // --- Rebuild every constraint in terms of the working columns ---
            for (int c = 0; c < model.Constraints.Count; c++)
            {
                Constraint original = model.Constraints[c];
                double[] workingCoeffs = new double[wm.VariableCount];

                for (int i = 0; i < n; i++)
                {
                    double coeff = original.Coefficients[i];
                    VariableMapping map = wm.OriginalVariableMappings[i];

                    if (map.Kind == MappingKind.Identity)
                    {
                        workingCoeffs[map.WorkingColumnPlus] = coeff;
                    }
                    else if (map.Kind == MappingKind.Negated)
                    {
                        workingCoeffs[map.WorkingColumnPlus] = -coeff;
                    }
                    else // SplitDifference
                    {
                        workingCoeffs[map.WorkingColumnPlus] = coeff;
                        workingCoeffs[map.WorkingColumnMinus] = -coeff;
                    }
                }

                wm.Constraints.Add(new Constraint(new List<double>(workingCoeffs), original.Relation, original.Rhs));
            }

            // --- Add the implicit upper bound (x <= 1) for every binary variable ---
            for (int b = 0; b < binWorkingColumns.Count; b++)
            {
                double[] boundRow = new double[wm.VariableCount];
                boundRow[binWorkingColumns[b]] = 1.0;
                wm.Constraints.Add(new Constraint(new List<double>(boundRow), RelationType.LessOrEqual, 1.0));
            }

            return wm;
        }

        // Builds the initial (iteration 0 / canonical form) Big-M simplex tableau from a WorkingModel.
        public static SimplexTableau BuildInitialTableau(WorkingModel wm)
        {
            int m = wm.Constraints.Count;
            int nOriginalWorking = wm.VariableCount;

            // Step A: normalise every row so its RHS is >= 0 (flip the row and its relation otherwise).
            List<double[]> normCoeffs = new List<double[]>();
            List<RelationType> normRelations = new List<RelationType>();
            List<double> normRhs = new List<double>();

            for (int i = 0; i < m; i++)
            {
                Constraint con = wm.Constraints[i];
                double[] coeffs = con.Coefficients.ToArray();
                RelationType relation = con.Relation;
                double rhs = con.Rhs;

                if (rhs < 0)
                {
                    for (int j = 0; j < coeffs.Length; j++)
                    {
                        coeffs[j] = -coeffs[j];
                    }
                    rhs = -rhs;
                    if (relation == RelationType.LessOrEqual) relation = RelationType.GreaterOrEqual;
                    else if (relation == RelationType.GreaterOrEqual) relation = RelationType.LessOrEqual;
                    // Equal stays Equal.
                }

                normCoeffs.Add(coeffs);
                normRelations.Add(relation);
                normRhs.Add(rhs);
            }

            // Step B: work out how many slack / surplus / artificial columns are needed.
            int extraColumns = 0;
            for (int i = 0; i < m; i++)
            {
                if (normRelations[i] == RelationType.LessOrEqual) extraColumns += 1;
                else if (normRelations[i] == RelationType.GreaterOrEqual) extraColumns += 2;
                else extraColumns += 1; // Equal -> artificial only
            }

            int totalColumns = nOriginalWorking + extraColumns;
            SimplexTableau tableau = new SimplexTableau(m, totalColumns);

            // Column names: original working variables first, then generated slack/surplus/artificial columns.
            for (int j = 0; j < nOriginalWorking; j++)
            {
                tableau.ColumnNames[j] = wm.VariableNames[j];
            }

            // Objective row (internally the tableau always MAXIMIZES; a minimisation problem's
            // coefficients are negated here and negated back again once a solution is found).
            double sign = wm.IsMaximize ? 1.0 : -1.0;
            for (int j = 0; j < nOriginalWorking; j++)
            {
                tableau.C[j] = sign * wm.ObjectiveCoefficients[j];
            }

            int col = nOriginalWorking;
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < nOriginalWorking; j++)
                {
                    tableau.A[i, j] = normCoeffs[i][j];
                }
                tableau.B[i] = normRhs[i];

                if (normRelations[i] == RelationType.LessOrEqual)
                {
                    tableau.A[i, col] = 1.0;
                    tableau.C[col] = 0.0;
                    tableau.ColumnNames[col] = "s" + (i + 1);
                    tableau.Basis[i] = col;
                    col = col + 1;
                }
                else if (normRelations[i] == RelationType.GreaterOrEqual)
                {
                    tableau.A[i, col] = -1.0;
                    tableau.C[col] = 0.0;
                    tableau.ColumnNames[col] = "e" + (i + 1);
                    col = col + 1;

                    tableau.A[i, col] = 1.0;
                    tableau.C[col] = -BigM;
                    tableau.ColumnNames[col] = "a" + (i + 1);
                    tableau.Basis[i] = col;
                    col = col + 1;
                }
                else // Equal
                {
                    tableau.A[i, col] = 1.0;
                    tableau.C[col] = -BigM;
                    tableau.ColumnNames[col] = "a" + (i + 1);
                    tableau.Basis[i] = col;
                    col = col + 1;
                }
            }

            tableau.OriginalWorkingVariableCount = nOriginalWorking;
            return tableau;
        }
    }
}
