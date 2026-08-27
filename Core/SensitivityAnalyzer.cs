using System;
using System.Collections.Generic;
using System.Text;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    public static class SensitivityAnalyzer
    {
        private const double Tolerance = 1e-9;

        //this would essentially be the final optimal table of a solved model
        public static SimplexTableau FinalTableau(SolveResult result) {
            return result.Iterations[result.Iterations.Count - 1];
        }

        //The initial (iteration 0)
        public static SimplexTableau InitialTableau(SolveResult result) {
            return result.Iterations[0];

        }

        public static int[] UnitColumnPerRow(SolveResult result)
        {
            return (int[])InitialTableau(result).Basis.Clone();
        }


        private static double EffectiveCost(SimplexTableau tableau, int columnIndex)
        {
            string name = tableau.ColumnNames[columnIndex];
            if (!string.IsNullOrEmpty(name) && name[0] == 'a')
            {
                return 0.0;
            }
            return tableau.C[columnIndex];
        }

        private static bool IsBasicColumn(SimplexTableau tableau, int column)
        {
            for (int i = 0; i < tableau.RowCount; i++)
            {
                if (tableau.Basis[i] == column) return true;
            }
            return false;
        }


        //Non-Basic Variable

        public static RangeResult RangeNonBasicVariable(SolveResult result, int columnIndex)
        {
            SimplexTableau t = FinalTableau(result);
            double[] cz = t.ComputeCjMinusZj();
            double zj = t.C[columnIndex] - cz[columnIndex];
            return new RangeResult { Lower = null, Upper = zj };
        }


        public static SimplexTableau ApplyNonBasicVariableChange(SolveResult result, int columnIndex, double newCoefficient)
        {
            SimplexTableau updated = FinalTableau(result).Clone();
            updated.C[columnIndex] = newCoefficient;
            return updated;
        }


        //Basic Variable
        public static RangeResult RangeBasicVariable(SolveResult result, int rowIndex)
        {
            SimplexTableau t = FinalTableau(result);
            double[] cz = t.ComputeCjMinusZj();
            int basisColumn = t.Basis[rowIndex];
            double currentCost = t.C[basisColumn];

            double? deltaLower = null;
            double? deltaUpper = null;

            for (int j = 0; j < t.ColumnCount; j++)
            {
                if (IsBasicColumn(t, j)) continue;

                double arj = t.A[rowIndex, j];
                if (System.Math.Abs(arj) < Tolerance) continue;

                double ratio = cz[j] / arj;
                if (arj > 0)
                {
                    if (deltaLower == null || ratio > deltaLower) deltaLower = ratio;
                }
                else
                {
                    if (deltaUpper == null || ratio < deltaUpper) deltaUpper = ratio;
                }
            }

            return new RangeResult
            {
                Lower = deltaLower.HasValue ? currentCost + deltaLower.Value : (double?)null,
                Upper = deltaUpper.HasValue ? currentCost + deltaUpper.Value : (double?)null
            };
        }

        public static SimplexTableau ApplyBasicVariableChange(SolveResult result, int rowIndex, double newCoefficient)
        {
            SimplexTableau updated = FinalTableau(result).Clone();
            updated.C[updated.Basis[rowIndex]] = newCoefficient;
            return updated; 
        }


        //Constraint RHS
        public static RangeResult RangeConstraintRhs(LPModel originalModel, SolveResult result, int constraintRow)
        {
            SimplexTableau t = FinalTableau(result);
            int bInvColumn = UnitColumnPerRow(result)[constraintRow];
            double currentRhs = originalModel.Constraints[constraintRow].Rhs;

            double? deltaLower = null;
            double? deltaUpper = null;

            for (int k = 0; k < t.RowCount; k++)
            {
                double coeff = t.A[k, bInvColumn]; // B^-1[k, constraintRow]
                if (System.Math.Abs(coeff) < Tolerance) continue;

                double ratio = -t.B[k] / coeff; // keeps the B[k] + delta*coeff >= 0
                if (coeff > 0)
                {
                    if (deltaLower == null || ratio > deltaLower) deltaLower = ratio;
                }
                else
                {
                    if (deltaUpper == null || ratio < deltaUpper) deltaUpper = ratio;
                }
            }

            return new RangeResult
            {
                Lower = deltaLower.HasValue ? currentRhs + deltaLower.Value : (double?)null,
                Upper = deltaUpper.HasValue ? currentRhs + deltaUpper.Value : (double?)null
            };
        }

        public static SimplexTableau ApplyConstraintRhsChange(LPModel originalModel, SolveResult result, int constraintRow, double newRhs)
        {
            SimplexTableau t = FinalTableau(result);
            SimplexTableau updated = t.Clone();
            int bInvColumn = UnitColumnPerRow(result)[constraintRow];
            double delta = newRhs - originalModel.Constraints[constraintRow].Rhs;

            for (int k = 0; k < updated.RowCount; k++)
            {
                updated.B[k] = t.B[k] + delta * t.A[k, bInvColumn];
            }
            return updated;
        }



        public static RangeResult RangeNonBasicColumnCoefficient(SolveResult result, double[] shadowPrices, int constraintRow, int nonBasicColumn)
        {
            SimplexTableau t = FinalTableau(result);
            double czOld = t.ComputeCjMinusZj()[nonBasicColumn];
            double sp = shadowPrices[constraintRow];
            double currentA = InitialTableau(result).A[constraintRow, nonBasicColumn];

            if (System.Math.Abs(sp) < Tolerance)
            {
                return new RangeResult { Lower = null, Upper = null };
            }

            double boundaryDelta = czOld / sp;
            if (sp > 0)
            {
                return new RangeResult { Lower = currentA + boundaryDelta, Upper = null };
            }
            return new RangeResult { Lower = null, Upper = currentA + boundaryDelta };
        }

        public static SimplexTableau ApplyNonBasicColumnCoefficientChange(SolveResult result, int constraintRow, int nonBasicColumn, double newCoefficient)
        {
            SimplexTableau t = FinalTableau(result);
            SimplexTableau updated = t.Clone();
            double delta = newCoefficient - InitialTableau(result).A[constraintRow, nonBasicColumn];
            int[] unitCols = UnitColumnPerRow(result);

            for (int k = 0; k < updated.RowCount; k++)
            {
                updated.A[k, nonBasicColumn] = t.A[k, nonBasicColumn] + delta * t.A[k, unitCols[constraintRow]];
            }
            return updated;
        }

        
  
        public static double[] ComputeShadowPrices(SolveResult result) { 
            SimplexTableau t = FinalTableau(result);
            int[] unitCols = UnitColumnPerRow(result);
            double[] prices = new double[unitCols.Length];
            for (int i = 0; i < unitCols.Length; i++)
            {
                double z = 0.0;
                for (int k = 0; k < t.RowCount; k++)
                {
                    z += EffectiveCost(t, t.Basis[k]) * t.A[k, unitCols[i]];
                }
                prices[i] = z;
            }
            return prices;
        }
    }
}
