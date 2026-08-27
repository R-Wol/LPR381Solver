using System;
using System.Collections.Generic;
using System.Text;

namespace LPR381Solver.Core
{
    public class RangeResult { 
        public double? Lower { get; set; }
        public double? Upper { get; set; }

        public string Describe() {
            string lowerText = Lower.HasValue ? System.Math.Round(Lower.Value, 3).ToString() : "-infinity";
            string upperText = Upper.HasValue ? System.Math.Round(Upper.Value, 3).ToString() : "+infinity";
            return "[" + lowerText + ", " + upperText + "]";
        }
    }
}
