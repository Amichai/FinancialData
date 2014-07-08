using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot.Series;
using OxyPlot;

namespace ChartingToolkit {
    public class Dataset1d : List<double> {

        private double? standardDeviation = null;
        public double StandardDev() {
            if (standardDeviation != null) return standardDeviation.Value;
            double ret = 0;
            if (this.Count() > 0) {
                //Compute the Average      
                double avg = this.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = this.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (this.Count() - 1));
            }
            this.standardDeviation = ret;
            return ret;
        }

        private double? average = null;
        public double Average {
            get {
                if (average == null) {
                    var ave = this.Average();
                    this.average = ave;
                    return ave;
                } else {
                    return average.Value;
                }
            }
        }

        public ScatterSeries GetScatterSeriesRankOrder(bool abs, bool logY) {
            List<double> pts2;
            if (abs) {
                pts2 = this.OrderByDescending(i => Math.Abs(i)).ToList();
            } else {
                pts2 = this.OrderByDescending(i => i).ToList();
            }
            ScatterSeries ls = new ScatterSeries();
            List<IDataPoint> data = new List<IDataPoint>();
            for (int i = 0; i < pts2.Count(); i++) {
                double val = pts2[i];
                if (abs) {
                    val = Math.Abs(val);
                }
                if (logY) {
                    val = Math.Log(val);
                }
                data.Add(new DataPoint(i, val));
            }
            ls.MarkerSize = 1;
            ls.MarkerFill = OxyColors.Red;
            ls.Points = data;
            return ls;
           }

        public ColumnSeries GetColumnSeries(double binSize) {
            Dictionary<int, int> idxQuantity = new Dictionary<int, int>();
            foreach (var a in this) {
                int binIdx = (int)Math.Floor(a / binSize);
                if (!idxQuantity.ContainsKey(binIdx)) {
                    idxQuantity[binIdx] = 1;
                } else {
                    idxQuantity[binIdx]++;
                }
            }

            var columnSeries = new ColumnSeries();

            var b = idxQuantity.OrderBy(i => i.Key);
            columnSeries.Tag = b.Select(i => (i.Key * binSize)).ToList();
            for (int i = 0; i < idxQuantity.Count(); i++) {
                columnSeries.Items.Add(new ColumnItem() {
                    CategoryIndex = i,
                    Value = b.ElementAt(i).Value,
                    Color = OxyColors.Green,
                });
            }
            return columnSeries;
        }
    }
}
