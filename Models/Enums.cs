namespace LPR381Solver.Models
{
    // Optimization direction of the objective function.
    public enum ObjectiveType
    {
        Maximize,
        Minimize
    }

    // Relational operator used in a constraint.
    public enum RelationType
    {
        LessOrEqual,
        GreaterOrEqual,
        Equal
    }

    // Sign restriction placed on a decision variable, as read from the input file.
    public enum SignRestriction
    {
        Positive,   // '+'   : x >= 0
        Negative,   // '-'   : x <= 0
        Urs,        // 'urs' : unrestricted in sign
        Int,        // 'int' : non-negative integer
        Bin         // 'bin' : binary (0 or 1)
    }

    // Outcome of running a simplex-based solver.
    public enum SolveStatus
    {
        Optimal,
        Infeasible,
        Unbounded
    }
}
