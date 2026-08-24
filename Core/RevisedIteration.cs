namespace LPR381Solver.Core;

// One snapshot of the Revised Primal Simplex Algorithm: the current basis,
// B^-1 (maintained as a running product of eta matrices - the "Product Form
// of the Inverse"), and the "Price Out" calculation (y = cB^T . B^-1, and the
// resulting Cj-Zj row) used to pick the entering variable for this iteration.
public class RevisedIteration
{
    public int IterationNumber { get; set; }
    public int[] Basis { get; set; } // column index basic in each row
    public double[,] BasisInverse { get; set; } // B^-1 (product of etas so far)
    public double[] XB { get; set; } // B^-1 . b, aligned with Basis rows
    public double[] Y { get; set; } // cB^T . B^-1 (simplex multipliers)
    public double[] CjMinusZj { get; set; } // priced-out row, one value per column
    
    public int EnteringColumn { get; set; } // -1 once optimal
    public double[] EnteringColumnBasisCoords { get; set; } // B^-1 . A_entering (Abar_j), null once optimal
    public int LeavingRow { get; set; } // -1 once optimal
    public double[] EtaColumn { get; set; } // eta vector used to update B^-1, null once optimal
    public double PivotValue { get; set; }

    public RevisedIteration()
    {
        EnteringColumn = -1;
        LeavingRow = -1;
    }
}