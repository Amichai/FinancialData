using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using OxyPlot.Series;

namespace Quant {
    public class Histogram {
		double binSize = 1;
		bool cumulative = false;
		List<double> positiveBins = new List<double>();
		List<double> negativeBins = new List<double>();
        List<RectangleBarItem> barItems = null;
		public Histogram(TimeSeries data, double binSize) {
			this.binSize = binSize;
			for (int i = 0; i < data.Count(); i++) {
				IncrementAt((int)Math.Floor(data[i] / binSize));
			}
		}

		/// <summary>Uses Scott's choice algorithm to assign bin width:
		/// http://en.wikipedia.org/wiki/Histogram#Number_of_bins_and_width</summary>
        public Histogram(TimeSeries data) {
			var bS = (3.5 * data.StandardDeviation) / Math.Pow(data.Count(), .33333333333333);
			if (bS <= 0)
				bS = .01;
			this.binSize = bS;
			for (int i = 0; i < data.Count(); i++) {
				IncrementAt((int)Math.Floor(data[i] / binSize));
			}
		}

        public Histogram(List<double> data) {
            var bS = (3.5 * data.StandardDev()) / Math.Pow(data.Count(), .33333333333333);
            if (bS <= 0)
                bS = .01;
            this.binSize = bS;
            for (int i = 0; i < data.Count(); i++) {
                IncrementAt((int)Math.Floor(data[i] / binSize));
            }
        }

		public Histogram Normalize() {
			for (int i = 0; i < positiveBins.Count(); i++) {
				positiveBins[i] /= totalNumberOfPoints;
			}
			for (int i = 0; i < negativeBins.Count(); i++) {
				negativeBins[i] /= totalNumberOfPoints;
			}
			return this;
		}

        public Histogram(TimeSeries data, double binSize, bool cumulative) {
			this.cumulative = cumulative;
			this.binSize = binSize;
			for (int i = 0; i < data.Count(); i++) {
				IncrementAt((int)Math.Floor(data[i] / binSize));
			}
		}

		private int totalNumberOfPoints = 0;

		private void IncrementAt(int idx) {
			if (idx > 100000)
				throw new Exception();
			totalNumberOfPoints++;
			if (idx > 0) {
				var dif = idx - positiveBins.Count();
				for (int i = 0; i <= dif; i++) {
					positiveBins.Add(0);
				}
				positiveBins[idx]++;
			} else {
				idx *= -1;
				var dif = idx - negativeBins.Count();
				for (int i = 0; i <= dif; i++) {
					negativeBins.Add(0);
				}
				negativeBins[idx]++;
			}
		}

		public double GetEntropy() {
			throw new NotImplementedException();
		}
		//Find the probability density function
		//assuming power law, and assuming exponential law

		public List<Tuple<double, double>> GetNonemptyPositiveBins() {
			List<Tuple<double, double>> outputDictionary = new List<Tuple<double, double>>();
			for(int i=0;i < positiveBins.Count(); i++) {
				var a = positiveBins[i];
				if (a != 0) {
					double xVal = Math.Round(i * binSize, 2);
					outputDictionary.Add(new Tuple<double, double>(xVal, a));
				}
			}
			return outputDictionary;
		}

        HistogramBin[] bins = null;

        public BarSeries PlotModel() {
            if (bins != null) {
                return new BarSeries{ ItemsSource = bins, ValueField = "Value"};
            }
            Random rand = new Random();
            bins = new HistogramBin[positiveBins.Count() + negativeBins.Count()];
            for (int i = 0; i < bins.Length; i++) {
                double value;
                if (i < negativeBins.Count()) {
                    value = negativeBins[i];
                } else {
                    value = positiveBins[i - negativeBins.Count()];
                }
                bins[i] = new HistogramBin { Label = (i - negativeBins.Count()) .ToString(), 
                 Value = value
                };
            }
           return new BarSeries{ ItemsSource = bins, ValueField = "Value"};
        }

        public RectangleBarSeries GetBarSeries() {
            
            var barSeries = new RectangleBarSeries();
            //barSeries.PlotModel = PlotModel();
            //barSeries.XAxis.PlotModel = PlotModel();
            barItems = new List<RectangleBarItem>();
            for (int i = 0; i < negativeBins.Count(); i++) {
                var item = (new RectangleBarItem(){ X0 = i * this.binSize, X1 = (i + 1) * binSize,  Y0 =0, Y1 = negativeBins[i]
                , Color = OxyColors.BlueViolet, Title = "Title"});
                barItems.Add(item);
            }
            barSeries.ItemsSource = barItems;
            return barSeries;
        }

        public void ShowGraph(string title = "") {
            var chart = new Chart();
            chart.AddSeries(this.PlotModel());
            chart.ShowUserControl();
        }
	}
    public class HistogramBin {
        public string Label { get; set; }
        public double Value { get; set; }
    }
}
