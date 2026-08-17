namespace LPR381Solver.Core
{
    // Describes how the value of an ORIGINAL decision variable is derived from
    // the WORKING variable(s) that replace it inside the standard/canonical form.
    public enum MappingKind
    {
        Identity,        // original value = working value                (Positive, Int, Bin restrictions)
        Negated,         // original value = -working value               (Negative restriction, x <= 0)
        SplitDifference  // original value = workingPlus - workingMinus   (Urs restriction)
    }

    public class VariableMapping
    {
        public MappingKind Kind { get; set; }
        public int WorkingColumnPlus { get; set; }   // working column index (also used for Identity/Negated)
        public int WorkingColumnMinus { get; set; }  // only used for SplitDifference, otherwise -1

        public VariableMapping(MappingKind kind, int workingColumnPlus, int workingColumnMinus)
        {
            Kind = kind;
            WorkingColumnPlus = workingColumnPlus;
            WorkingColumnMinus = workingColumnMinus;
        }

        // Recovers the value of the original variable from the solved working-variable values.
        public double ResolveValue(double[] workingValues)
        {
            if (Kind == MappingKind.Identity)
            {
                return workingValues[WorkingColumnPlus];
            }
            if (Kind == MappingKind.Negated)
            {
                return -workingValues[WorkingColumnPlus];
            }
            // SplitDifference
            return workingValues[WorkingColumnPlus] - workingValues[WorkingColumnMinus];
        }
    }
}
