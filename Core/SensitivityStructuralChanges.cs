using System;
using System.Collections.Generic;
using System.Text;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{

    public static class SensitivityStructuralChanges
    {
        public static SolveResult AddNewActivity(LPModel originalModel, double objectiveCoefficient, double[] constraintCoefficients, SignRestriction restriction)
        {
            LPModel clone = CloneModel(originalModel);
            clone.ObjectiveCoefficients.Add(objectiveCoefficient);
            clone.SignRestrictions.Add(restriction);

            for (int i = 0; i < clone.Constraints.Count; i++)
            {
                clone.Constraints[i].Coefficients.Add(constraintCoefficients[i]);
            }

            return PrimalSimplexSolver.Solve(clone);
        }

        public static SolveResult AddNewConstraint(LPModel originalModel, double[] coefficients, RelationType relation, double rhs)
        {
            LPModel clone = CloneModel(originalModel);
            clone.Constraints.Add(new Constraint(new List<double>(coefficients), relation, rhs));
            return PrimalSimplexSolver.Solve(clone);
        }

        private static LPModel CloneModel(LPModel source)
        {
            LPModel clone = new LPModel();
            clone.ObjectiveType = source.ObjectiveType;
            clone.ObjectiveCoefficients = new List<double>(source.ObjectiveCoefficients);
            clone.SignRestrictions = new List<SignRestriction>(source.SignRestrictions);
            clone.Constraints = new List<Constraint>();
            for (int i = 0; i < source.Constraints.Count; i++)
            {
                clone.Constraints.Add(source.Constraints[i].Clone());
            }
            return clone;
        }
    }
}
