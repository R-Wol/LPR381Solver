using System.Text;

namespace LPR381Solver.Core
{
    // Holds one snapshot (one iteration) of a simplex tableau: the constraint matrix A,
    // the right-hand-side vector B, the objective-row coefficients C for every column,
    // which column is basic in every row, and each column's display name.
    public class SimplexTableau
    {
        public double[,] A;
        public double[] B;
        public double[] C;
        public int[] Basis;
        public string[] ColumnNames;
        public int OriginalWorkingVariableCount;

        public int RowCount
        {
            get { return B.Length; }
        }

        public int ColumnCount
        {
            get { return C.Length; }
        }

        public SimplexTableau(int rows, int columns)
        {
            A = new double[rows, columns];
            B = new double[rows];
            C = new double[columns];
            Basis = new int[rows];
            ColumnNames = new string[columns];
        }

        // Deep copy - a new snapshot is stored after every pivot so the full iteration
        // history can be displayed / written to the output file.
        public SimplexTableau Clone()
        {
            SimplexTableau copy = new SimplexTableau(RowCount, ColumnCount);
            for (int i = 0; i < RowCount; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    copy.A[i, j] = A[i, j];
                }
                copy.B[i] = B[i];
                copy.Basis[i] = Basis[i];
            }
            for (int j = 0; j < ColumnCount; j++)
            {
                copy.C[j] = C[j];
                copy.ColumnNames[j] = ColumnNames[j];
            }
            copy.OriginalWorkingVariableCount = OriginalWorkingVariableCount;
            return copy;
        }

        // The Cj - Zj row used both to test optimality and to pick the entering variable.
        public double[] ComputeCjMinusZj()
        {
            double[] cb = new double[RowCount];
            for (int i = 0; i < RowCount; i++)
            {
                cb[i] = C[Basis[i]];
            }

            double[] result = new double[ColumnCount];
            for (int j = 0; j < ColumnCount; j++)
            {
                double zj = 0.0;
                for (int i = 0; i < RowCount; i++)
                {
                    zj += cb[i] * A[i, j];
                }
                result[j] = C[j] - zj;
            }
            return result;
        }

        // Current objective value = cB . B
        public double ComputeObjectiveValue()
        {
            double total = 0.0;
            for (int i = 0; i < RowCount; i++)
            {
                total += C[Basis[i]] * B[i];
            }
            return total;
        }

        // Formats this tableau as an aligned text table, rounding every value to 3 decimal places.
        public string ToDisplayString(int iterationNumber)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("--- Iteration ").Append(iterationNumber).Append(" ---").Append("\n");

            sb.Append("Basis");
            for (int j = 0; j < ColumnCount; j++)
            {
                sb.Append("\t").Append(ColumnNames[j]);
            }
            sb.Append("\tRHS").Append("\n");

            for (int i = 0; i < RowCount; i++)
            {
                sb.Append(ColumnNames[Basis[i]]);
                for (int j = 0; j < ColumnCount; j++)
                {
                    sb.Append("\t").Append(Round3(A[i, j]));
                }
                sb.Append("\t").Append(Round3(B[i])).Append("\n");
            }

            double[] cz = ComputeCjMinusZj();
            sb.Append("Cj-Zj");
            for (int j = 0; j < ColumnCount; j++)
            {
                sb.Append("\t").Append(Round3(cz[j]));
            }
            sb.Append("\tObj=").Append(Round3(ComputeObjectiveValue())).Append("\n");

            return sb.ToString();
        }

        private static double Round3(double value)
        {
            return System.Math.Round(value, 3);
        }
    }
}
