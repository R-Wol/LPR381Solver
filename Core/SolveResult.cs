using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    // The full result of running a simplex-based algorithm: every tableau iteration
    // (for display / the output file), the final status, and - when optimal - the
    // solution mapped back to the ORIGINAL decision variables the user entered.
    public class SolveResult
    {
        public SolveStatus Status { get; set; }
        public List<SimplexTableau> Iterations { get; set; }
        public double[] OriginalVariableValues { get; set; }
        public double ObjectiveValue { get; set; }

        public SolveResult()
        {
            Iterations = new List<SimplexTableau>();
        }
    }
}
