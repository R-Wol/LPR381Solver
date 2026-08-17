using System.Collections.Generic;

namespace LPR381Solver.Models
{
    // A single linear constraint: Coefficients . x  {<=, >=, =}  Rhs
    public class Constraint
    {
        public List<double> Coefficients { get; set; }
        public RelationType Relation { get; set; }
        public double Rhs { get; set; }

        public Constraint(List<double> coefficients, RelationType relation, double rhs)
        {
            Coefficients = coefficients;
            Relation = relation;
            Rhs = rhs;
        }

        // Deep copy - used whenever a constraint needs to be transformed without mutating the original.
        public Constraint Clone()
        {
            List<double> copy = new List<double>(Coefficients);
            return new Constraint(copy, Relation, Rhs);
        }
    }
}
