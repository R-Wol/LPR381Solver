using System.Collections.Generic;
using LPR381Solver.Models;

namespace LPR381Solver.Core
{
    // An intermediate LP where every variable is a plain non-negative variable (x >= 0).
    // Built from the user's LPModel by:
    //   - substituting  x = -x'            for variables restricted to be <= 0 ('-')
    //   - splitting     x = xPlus - xMinus for unrestricted variables ('urs')
    //   - adding an explicit  x <= 1  constraint for binary variables ('bin')
    // 'int' and '+' variables pass through unchanged (integrality for 'int'/'bin' is
    // enforced later by the Branch & Bound solvers, not at this LP-relaxation stage).
    public class WorkingModel
    {
        public bool IsMaximize { get; set; }
        public List<double> ObjectiveCoefficients { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<string> VariableNames { get; set; }
        public List<VariableMapping> OriginalVariableMappings { get; set; }

        public WorkingModel()
        {
            ObjectiveCoefficients = new List<double>();
            Constraints = new List<Constraint>();
            VariableNames = new List<string>();
            OriginalVariableMappings = new List<VariableMapping>();
        }

        public int VariableCount
        {
            get { return ObjectiveCoefficients.Count; }
        }
    }
}
