using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using System.Diagnostics;

namespace Quant {
    public class StraightRun {
        public StraightRun(double rise, IEnumerable<double> data, double slope, double range) {
            this.Rise = rise;
            this.Data = data;
            this.Variance = 0;
            for (int i = 0; i < data.Count(); i++) {
                Variance += data.ElementAt(i) - i * slope;
            }
            if (Variance == 0) throw new Exception("divide by zero");
            //Debug.Print(Variance.ToString());
            this.Interestingness = (Math.Abs(Rise)) / Variance;
        }

        public double Rise { get; set; }
        public DateTime Range { get; set; }
        IEnumerable<double> Data;
        public double Variance { get; set; }
        public double Interestingness { get; set; }
    }
}
