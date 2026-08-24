using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core;
// Result of running the Revised Primal Simplex Algorithm: every iteration's
// Product Form / Price Out snapshot, the final status, and - when optimal -
// the solution mapped back to the ORIGINAL decision variables.
public class RevisedSolveResult
{
    public SolveStatus Status { get; set; }
    public List<RevisedIteration> Iterations { get; set; }
    public double[] OriginalVariableValues { get; set; }
    public double ObjectiveValue { get; set; }
    public string[] ColumnNames { get; set; } // used by the output writer to label columns

    public RevisedSolveResult()
    {
        Iterations = new List<RevisedIteration>();
    }
}