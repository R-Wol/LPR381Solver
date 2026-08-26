using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core;

public static class CuttingPlaneSolver
{
    private const double Tolerance = 1e-6;
    private const int MaxCuts = 50;

    public static CuttingPlaneResult Solve(LPModel model)
    {
        WorkingModel workingModel = StandardFormBuilder.BuildWorkingModel(model);
        int nWorking = workingModel.VariableCount;

        List<int> integerVariableIndices = new List<int>();
        for (int i = 0; i < model.VariableCount; i++)
        {
            SignRestriction r = model.SignRestrictions[i];
            if (r == SignRestriction.Int || r == SignRestriction.Bin)
            {
                integerVariableIndices.Add(i);
            }
        }

        CuttingPlaneResult result = new CuttingPlaneResult();
        List<Constraint> cuts = new List<Constraint>();

        for (int roundNumber = 0; roundNumber < MaxCuts; roundNumber++)
        {
            WorkingModel roundModel = CloneWithExtraConstraints(workingModel, cuts);
            SimplexTableau initialTableau = StandardFormBuilder.BuildInitialTableau(roundModel);
            RevisedSolveResult relaxation = RevisedPrimalSimplexSolver.SolveWorkingModel(roundModel, model.VariableCount);
            
            CuttingPlaneRound round = new CuttingPlaneRound();
            round.RoundNumber = roundNumber;
            round.Relaxation = relaxation;
            result.Rounds.Add(round);

            if (relaxation.Status != SolveStatus.Optimal)
            {
                result.Status = relaxation.Status;
                return result;
            }

            int fractionalIndex = -1;
            for (int k = 0; k < integerVariableIndices.Count; k++)
            {
                int idx = integerVariableIndices[k];
                double value = relaxation.OriginalVariableValues[idx];
                double nearest = System.Math.Round(value);
                if (System.Math.Abs(value - nearest) > Tolerance)
                {
                    fractionalIndex = idx;
                    break;
                }
            }

            if (fractionalIndex == -1)
            {
                round.IsIntegerFeasible = true;
                result.Status = SolveStatus.Optimal;
                result.ObjectiveValue = relaxation.ObjectiveValue;
                result.OriginalVariableValues = relaxation.OriginalVariableValues;
                return result;
            }
            
            round.FractionalVariableIndex = fractionalIndex;

            int workingColumn = workingModel.OriginalVariableMappings[fractionalIndex].WorkingColumnPlus;

            RevisedIteration finalIteration = relaxation.Iterations[relaxation.Iterations.Count - 1];
            int rowIndex = -1;
            for (int i = 0; i < finalIteration.Basis.Length; i++)
            {
                if (finalIteration.Basis[i] == workingColumn)
                {
                    rowIndex = i;
                    break;
                }
            }

            double[] cutCoefficients;
            double cutRhs;
            DeriveGomoryCut(rowIndex, finalIteration.Basis, finalIteration.BasisInverse, initialTableau.A, initialTableau.B, initialTableau.ColumnNames, nWorking, out cutCoefficients, out cutRhs);
            round.CutCoefficients = cutCoefficients;
            round.CutRhs = cutRhs;
            
            cuts.Add(new Constraint(coefficients:new List<double>(cutCoefficients), RelationType.GreaterOrEqual, cutRhs));
            
        }

        result.CutLimitReached = true;
        return result;
    }

    private static double SafeFrac(double x)
    {
        double nearest = System.Math.Round(x);
        if (System.Math.Abs(x - nearest) < 1e-9)
        {
            return 0.0;
        }

        return x - System.Math.Floor(x);
    }

    private static void DeriveGomoryCut(int rowIndex, int[] basis, double[,] basisInverse, double[,] initialA,
        double[] initialB, string[] colNames, int nWorking, out double[] workingCutCoeffients, out double cutRhs)
    {
        int totalColumns = colNames.Length;

        double[] tableauRow = new double[totalColumns];
        for (int j = 0; j < totalColumns; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < basis.Length; k++)
            {
                sum += basisInverse[rowIndex, k] * initialA[k, j];
            }

            tableauRow[j] = sum;
        }

        double bHat = 0.0;
        for (int k = 0; k < basis.Length; k++)
        {
            bHat += basisInverse[rowIndex, k] * initialB[k];
        }
        
        double fi = SafeFrac(bHat);
        double[] workingCoeffs = new double[nWorking];
        double rhsConstant = 0.0;

        int basicColumnThisRow = basis[rowIndex];

        for (int j = 0; j < totalColumns; j++)
        {
            if (j == basicColumnThisRow) continue;
            
            double aij = tableauRow[j];
            double fij = SafeFrac(aij);
            if (fij < 1e-9) continue;

            if (j < nWorking)
            {
                workingCoeffs[j] += fij;
            }
            else
            {
                string name = colNames[j];
                if (name.StartsWith("s"))
                {
                    int k = int.Parse(name.Substring(1)) - 1;
                    for (int m = 0; m < nWorking; m++)
                    {
                        workingCoeffs[m] += fij * initialA[k, m];
                    }
                    rhsConstant -= fij * initialB[k];
                }
            }
        }

        workingCutCoeffients = workingCoeffs;
        cutRhs = fi - rhsConstant;
    }

    private static WorkingModel CloneWithExtraConstraints(WorkingModel original, List<Constraint> extraConstraints)
    {
        WorkingModel clone = new WorkingModel();
        clone.IsMaximize = original.IsMaximize;
        clone.ObjectiveCoefficients = new List<double>(original.ObjectiveCoefficients);
        clone.VariableNames = new List<string>(original.VariableNames);
        clone.OriginalVariableMappings = original.OriginalVariableMappings;

        clone.Constraints = new List<Constraint>();
        for (int i = 0; i < original.Constraints.Count; i++)
        {
            clone.Constraints.Add(original.Constraints[i].Clone());
        }

        for (int i = 0; i < extraConstraints.Count; i++)
        {
            clone.Constraints.Add(extraConstraints[i].Clone());
        }

        return clone;
    }
}