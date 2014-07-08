using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot.Axes;
using OxyPlot;
using OxyPlot.Series;

namespace ChartingToolkit {
    public class TimeSeries {
        public TimeSeries() {
            this.MaxValue = double.MinValue;
            this.MinValue = double.MinValue;
            this.dataPoints = new List<IDataPoint>();
            this.Domain = new TimeDomain();
            this.Range = new Dataset1d();
        }

        public double MaxValue { get; private set; }
        public double MinValue { get; private set; }

        private TimeDomain Domain { get; set; }
        private Dataset1d Range { get; set; }
        IList<IDataPoint> dataPoints;

        public double TotalChange {
            get {
                return EndVal - StartVal;
            }
        }

        public double PercentChange {
            get {
                return (TotalChange / StartVal) * 100.0;
            }
        }

        public double StartVal {
            get {
                return Range.Last();
            }
        }

        public double EndVal {
            get {
                return Range.First();
            }
        }

        public DateTime StartDate {
            get {
                return DateTimeAxis.ToDateTime(Domain.Last());
            }
        }

        public DateTime EndDate {
            get {
                return DateTimeAxis.ToDateTime(Domain.First());
            }
        }

        public TimeSeries GetDiffs() {
            double lastVal = double.NaN;
            TimeSeries ts = new TimeSeries();
            ///Notice that this for loop iterates forward in time
            for (int i = Domain.Count() - 1; i >= 0; i--) {
                double y = Range[i] / lastVal - 1;
                if (double.IsNaN(lastVal)) y = 0;
                ts.Add(Domain[i], y);
                lastVal = Range[i];
            }
            return ts;
        }

        public IEnumerable<double> RangeForwardInTime() {
            for (int i = Domain.Count() - 1; i >= 0; i--) {
                yield return Range[i];
            }
        }

        public LineSeries GetLineSeries(string title = "") {
            var ls = new LineSeries() { StrokeThickness = .5, CanTrackerInterpolatePoints = false };
            ls.Points = dataPoints;
            return ls;
        }

        public void Add(double x, double y) {
            Domain.Add(x);
            Range.Add(y);
            this.dataPoints.Add(new DataPoint(x, y));
            if (y < MinValue) {
                MinValue = y;
            }
            if (y > MaxValue) {
                MaxValue = y;
            }
        }

        public void Add(DateTime x, double y) {
            var asDouble = DateTimeAxis.ToDouble(x);
            if (double.IsNaN(asDouble) || double.IsNaN(y)) {
                throw new Exception("Not a number");
            }
            Add(asDouble, y);
        }
    }
}
