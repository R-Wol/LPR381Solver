using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core;

public class CuttingPlaneRound
{
    public int RoundNumber { get; set; }
    public RevisedSolveResult Relaxation { get; set; }
    public bool IsIntegerFeasible { get; set; }
    public int FractionalVariableIndex { get; set; }
    public double[] CutCoefficients { get; set; }
    public double CutRhs { get; set; }

    public CuttingPlaneRound()
    {
        FractionalVariableIndex = -1;
    }
}

public class CuttingPlaneResult
{
    public SolveStatus Status { get; set; }
    public double ObjectiveValue { get; set; }
    public double[] OriginalVariableValues { get; set; }
    public List<CuttingPlaneRound> Rounds { get; set; }
    public bool CutLimitReached { get; set; }

    public CuttingPlaneResult()
    {
        Rounds = new List<CuttingPlaneRound>();
    }
}