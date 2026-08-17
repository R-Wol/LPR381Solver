using System.Collections.Generic;

namespace LPR381Solver.Models
{
    // The parsed Linear/Integer Programming model exactly as specified in the input file.
    // This is NOT a canonical/standard form - see Core/StandardFormBuilder for that conversion.
    public class LPModel
    {
        public ObjectiveType ObjectiveType { get; set; }
        public List<double> ObjectiveCoefficients { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<SignRestriction> SignRestrictions { get; set; }

        public LPModel()
        {
            ObjectiveCoefficients = new List<double>();
            Constraints = new List<Constraint>();
            SignRestrictions = new List<SignRestriction>();
        }

        // Number of original decision variables (as they appear in the input file).
        public int VariableCount
        {
            get { return ObjectiveCoefficients.Count; }
        }

        // Display names x1..xn for the original decision variables.
        public List<string> VariableNames
        {
            get
            {
                List<string> names = new List<string>();
                for (int i = 1; i <= VariableCount; i++)
                {
                    names.Add("x" + i);
                }
                return names;
            }
        }
    }
}
