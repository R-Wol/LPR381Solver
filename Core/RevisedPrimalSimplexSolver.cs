using LPR381Solver.Models;

namespace LPR381Solver.Core;

// The Revised Primal Simplex Algorithm: maintains only B^-1 (as a running
// product of elementary "eta" matrices - the Product Form of the Inverse)
// and prices out every non-basic column on the fly each iteration, instead
// of keeping the full updated tableau the way PrimalSimplexSolver does.
// Uses the exact same Big-M standard form as the tableau method (built once
// via StandardFormBuilder and never mutated afterwards) so both algorithms
// are solving an identical problem and should reach an identical answer.

public static class RevisedPrimalSimplexSolver
{
    private const double Tolerance = 1e-9;
    private const int MaxIterations = 1000;

    public static RevisedSolveResult Solve(LPModel model)
    {
        WorkingModel workingModel = StandardFormBuilder.BuildWorkingModel(model);
        return SolveWorkingModel(workingModel, model.VariableCount);
    }

    public static RevisedSolveResult SolveWorkingModel(WorkingModel workingModel, int originalVariableCount)
    {
        SimplexTableau initial = StandardFormBuilder.BuildInitialTableau(workingModel);

        int m = initial.RowCount;
        int n = initial.ColumnCount;
        double[,] A = initial.A;
        double[] rhs = initial.B;
        double[] c = initial.C;
        string[] colNames = initial.ColumnNames;

        int[] basis = new int[m];
        for(int i = 0; i < m; i++) basis[i] = initial.Basis[i];

        double[,] basisInverse = Identify(m);
        
        RevisedSolveResult result = new RevisedSolveResult();
        result.ColumnNames = colNames;

        int iterationCount = 0;
        while (true)
        {
            RevisedIteration snap =  new RevisedIteration();
            snap.IterationNumber = iterationCount;
            snap.Basis = (int[])basis.Clone();
            snap.BasisInverse = CloneMatrix(basisInverse);

            double[] cb = new double[m];
            for (int i = 0; i < m; i++) cb[i] = c[basis[i]];
            
            double[] y = VectorTimesMatrix(cb, basisInverse); // "price out" vector
            double[] xB = MatrixTimesVector(basisInverse, rhs);

            snap.Y = y;
            snap.XB = xB;
            
            double[] cz = new double[n];
            for (int j = 0; j < n; j++)
            {
                double[] colJ = GetColumn(A, rowCount: m, columnIndex: j);
                double zj = DotProduct(left: y, right: colJ);
                cz[j] = c[j] - zj;
            }

            snap.CjMinusZj = cz;

            int entering = -1;
            double best = Tolerance;
            for (int j = 0; j < n; j++)
            {
                if (IsBasis(basis, j)) continue;
                if (cz[j] > best)
                {
                    best = cz[j];
                    entering = j;
                }
            }

            if (entering == -1)
            {
                result.Iterations.Add(snap);
                break;
            }

            double[] enteringColumnRaw = GetColumn(A, rowCount: m, columnIndex: entering);
            double[] abarJ = MatrixTimesVector(basisInverse, enteringColumnRaw);
            snap.EnteringColumn = entering;
            snap.EnteringColumnBasisCoords = abarJ;
            
            int leavingRow = -1;
            double bestRatio = 0.0;
            for (int i = 0; i < m; i++)
            {
                if (abarJ[i] > Tolerance)
                {
                    double ratio = xB[i] /  abarJ[i];
                    if (leavingRow == -1 || ratio > bestRatio - 1e-12)
                    {
                        bestRatio = ratio;
                        leavingRow = i;
                    }
                }
            }

            if (leavingRow == -1)
            {
                result.Iterations.Add(snap);
                result.Status = SolveStatus.Unbounded;
                return result;
            }
            
            snap.LeavingRow = leavingRow;
            
            double pivotValue = abarJ[leavingRow];
            double[] eta = new double[m];
            for (int i = 0; i < m; i++)
            {
                eta[i] = -abarJ[i] / pivotValue;
            }

            eta[leavingRow] = 1.0 / pivotValue;
            snap.EtaColumn = eta;
            snap.PivotValue = pivotValue;
            
            result.Iterations.Add(snap);
            
            // Product form update: B^-1_new = E * B^-1_old, where E is identity
            // except column 'leavingRow' replaced with the eta vector above.
            double[,] E = Identify(m);
            for (int i = 0; i < m; i++)
            {
                E[i, leavingRow] = eta[i];
            }

            basisInverse = MultiplyMatrices(left: E, right: basisInverse, m);

            basis[leavingRow] = entering;
            iterationCount = iterationCount + 1;

            if (iterationCount > MaxIterations)
            {
                result.Status = SolveStatus.Unbounded;
                return result;
            }
        }
        
        // Feasibility check: any artificial variable still basic at a
        // positive value means the original problem has no feasible solution.
        double[] finalXB = MatrixTimesVector(basisInverse, rhs);
        for (int i = 0; i < m; i++)
        {
            string basicName = colNames[basis[i]];
            if (basicName.StartsWith("a") && finalXB[i] > 1e-6)
            {
                result.Status = SolveStatus.Infeasible;
                return result;
            }
        }
        
        // Extract the working-variable solution, then map it back to the
        // original decision variables. 
        double[] workingValues = new double[workingModel.VariableCount];
        for (int i = 0; i < m; i++)
        {
            if (basis[i] < workingModel.VariableCount)
            {
                workingValues[basis[i]] = finalXB[i];
            }
        }

        double[] originalValues = new double[originalVariableCount];
        for (int i = 0; i < originalVariableCount; i++)
        {
            originalValues[i] = workingModel.OriginalVariableMappings[i].ResolveValue(workingValues);
        }

        double[] finalCb = new double[m];
        for(int i = 0; i < m; i++) finalCb[i] = c[basis[i]];
        double internalObjective = DotProduct(left: finalCb, right:finalXB);
        double finalObjective = workingModel.IsMaximize ? internalObjective : -internalObjective;

        result.Status = SolveStatus.Optimal;
        result.OriginalVariableValues = originalValues;
        result.ObjectiveValue = System.Math.Round(finalObjective, 3);
        return result;
    }

    private static double[,] Identify(int size)
    {
        double[,] result = new double[size, size];
        for (int i = 0; i < size; i++) result[i, i] = 1.0;
        return result;
    }

    private static double[,] CloneMatrix(double[,] source)
    {
        int rows = source.GetLength(dimension: 0);
        int cols = source.GetLength(dimension: 0);
        double[,] copy = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                copy[i, j] = source[i, j];
            }
        }
        return copy;
    }

    private static bool IsBasis(int[] basis, int column)
    {
        for (int i = 0; i < basis.Length; i++)
        {
            if (basis[i] == column) return true;
        }

        return false;
    }

    private static double[] GetColumn(double[,] matrix, int rowCount, int columnIndex)
    {
        double[] column = new double[rowCount];
        for (int i = 0; i < rowCount; i++)
        {
            column[i] = matrix[i,  columnIndex];
        }
        return column;
    }

    private static double[] MatrixTimesVector(double[,] matrix, double[] vector)
    {
        int rows = matrix.GetLength(dimension: 0);
        int cols = matrix.GetLength(dimension: 1);
        double[] result = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double sum = 0.0;
            for (int j = 0; j < cols; j++)
            {
                sum += matrix[i, j] * vector[j];
            }
            result[i] = sum;
        }
        return result;
    }

    private static double[] VectorTimesMatrix(double[] rowVector, double[,] matrix)
    {
        int rows = matrix.GetLength(dimension: 0);
        int cols = matrix.GetLength(dimension: 1);
        double[] result = new double[cols];
        for (int j = 0; j < cols; j++)
        {
            double sum = 0.0;
            for (int i = 0; i < rows; i++)
            {
                sum += rowVector[i] * matrix[i, j];
            }
            result[j] = sum;
        }
        return result;
    }

    private static double[,] MultiplyMatrices(double[,] left, double[,] right, int size)
    {
        double[,] result = new double[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                double sum = 0.0;
                for (int k = 0; k < size; k++)
                {
                    sum += left[i, k] * right[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }

    private static double DotProduct(double[] left, double[] right)
    {
        double sum = 0.0;
        for (int i = 0; i < left.Length; i++)
        {
            sum += left[i] * right[i];
        }
        return sum;
    }
}