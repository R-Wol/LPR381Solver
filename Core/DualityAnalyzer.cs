using System;
using System.Collections.Generic;
using System.Text;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
  
    public static class DualityAnalyzer
    {
        public static LPModel BuildDualModel(LPModel primal)
        {
            Validate(primal);

            int m = primal.Constraints.Count;
            int n = primal.VariableCount;

            LPModel dual = new LPModel();
            dual.ObjectiveType = ObjectiveType.Minimize;

            dual.ObjectiveCoefficients = new List<double>();
            for (int i = 0; i < m; i++)
            {
                dual.ObjectiveCoefficients.Add(primal.Constraints[i].Rhs);
            }

            dual.SignRestrictions = new List<SignRestriction>();
            for (int i = 0; i < m; i++)
            {
                dual.SignRestrictions.Add(SignRestriction.Positive);
            }

            dual.Constraints = new List<Constraint>();
            for (int j = 0; j < n; j++)
            {
                double[] coeffs = new double[m];
                for (int i = 0; i < m; i++)
                {
                    coeffs[i] = primal.Constraints[i].Coefficients[j];
                }
                dual.Constraints.Add(new Constraint(new List<double>(coeffs), RelationType.GreaterOrEqual, primal.ObjectiveCoefficients[j]));
            }

            return dual;
        }

        public static SolveResult SolveDual(LPModel primal)
        {
            LPModel dual = BuildDualModel(primal);
            return PrimalSimplexSolver.Solve(dual);
        }
        //losing my mind
        public static string VerifyDuality(SolveResult primalResult, SolveResult dualResult)
        {
            if (primalResult.Status != SolveStatus.Optimal || dualResult.Status != SolveStatus.Optimal)
            {
                return "Cannot verify duality - primal and/or dual did not reach an optimal solution.";
            }

            double diff = System.Math.Abs(primalResult.ObjectiveValue - dualResult.ObjectiveValue);
            if (diff < 1e-3)
            {
                return "Strong duality holds: primal objective (" + primalResult.ObjectiveValue +
                       ") equals dual objective (" + dualResult.ObjectiveValue + ").";
            }
            return "Only weak duality observed: primal objective (" + primalResult.ObjectiveValue +
                   ") and dual objective (" + dualResult.ObjectiveValue + ") differ - check model formulation.";
        }

        private static void Validate(LPModel primal)
        {
            if (primal.ObjectiveType != ObjectiveType.Maximize)
            {
                throw new System.FormatException("This Duality builder currently supports maximization primal models only.");
            }
            for (int i = 0; i < primal.Constraints.Count; i++)
            {
                if (primal.Constraints[i].Relation != RelationType.LessOrEqual)
                {
                    throw new System.FormatException("This Duality builder currently supports '<=' constraints only.");
                }
            }
            for (int j = 0; j < primal.VariableCount; j++)
            {
                SignRestriction r = primal.SignRestrictions[j];
                if (r != SignRestriction.Positive && r != SignRestriction.Int && r != SignRestriction.Bin)
                {
                    throw new System.FormatException("This Duality builder currently supports non-negative (+/int/bin) variables only.");
                }
            }
        }
    }
}
