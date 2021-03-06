﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OxyPlot;
using System.Diagnostics;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Quant {
    public class TimeSeries {
        List<double> domain;
        List<double> range;
        List<DataPoint> points;
        public string Name;
        public TimeSeries(string name) {
            this.domain = new List<double>();
            this.range = new List<double>();
            this.points = new List<DataPoint>();
            this.Name = name;
        }

        public double MaxVal = double.MinValue;
        public double MinVal = double.MaxValue;

        public void Add(double x, double y) {
            this.domain.Add(x);
            this.range.Add(y);
            if (double.IsNaN(x) || double.IsNaN(y)) {
                throw new Exception("Non a number");
            }
            this.points.Add(new DataPoint(x, y));
            if (y < MinVal) {
                MinVal = y;
            }
            if (y > MaxVal) {
                MaxVal = y;
            }
        }

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
                return range.Last();
            }
        }

        public double EndVal {
            get {
                return range.First();
            }
        }

        public DateTime StartDate {
            get {
                return DateTimeAxis.ToDateTime(domain.Last());
            }
        }

        public DateTime EndDate {
            get {
                return DateTimeAxis.ToDateTime(domain.First());
            }
        }

        public void LogarithmicX() {
            points.ForEach(i => i.X = Math.Log(i.X));
        }

        public void LogarithmicY() {
            points.ForEach(i => i.Y = Math.Log(i.Y));
        }

        public TimeSeries GetDiffs() {
            double lastVal = double.NaN;
            TimeSeries ts = new TimeSeries(this.Name);
            ///Notice that this for loop iterates forward in time
            for (int i = domain.Count() - 1; i >= 0; i--) {
                double y = range[i] / lastVal - 1;
                if (double.IsNaN(lastVal)) y = 0;
                ts.Add(domain[i], y);
                lastVal = range[i];
            }
            return ts;
        }

        public IEnumerable<double> RangeForwardInTime() {
            for (int i = domain.Count() - 1; i >= 0; i--) {
                yield return range[i];
            }
        }

        private double maxDrawDown = double.MinValue;
        public double MaxDrawDown {
            get {
                if (maxDrawDown != double.MinValue) {
                    return maxDrawDown;
                }
                var a = DrawDowns;
                return maxDrawDown;
            }
        }

        enum sign { plus, minus, equal };

        private List<double> drawDown2 = null;
        public List<double> DrawDown2 {
            get {
                if (drawDown2 != null) return drawDown2;
                List<double> ts = new List<double>();
                sign currentSign = sign.equal;
                sign lastSign = sign.equal;
                double runningSum = 0;
                for (int i = DailyReturns.Count() - 1; i >= 0; i--) {
                    var ret = DailyReturns[i];
                    if (ret > 0) currentSign = sign.plus;
                    if (ret < 0) currentSign = sign.minus;
                    if (ret == 0) currentSign = sign.equal;
                    if (currentSign != sign.equal && currentSign != lastSign && lastSign != sign.equal) {
                        //run just broke
                        ts.Add(runningSum);
                        runningSum = 0;
                    } else {
                        runningSum += ret;
                    }
                    lastSign = currentSign;
                }
                return ts;
            }
        }


        private TimeSeries drawDowns = null;
        public TimeSeries DrawDowns {
            get {
                if (drawDowns != null) return drawDowns;
                TimeSeries ts = new TimeSeries(Name);
                double peak = double.MinValue;
                for (int i = domain.Count() - 1; i >= 0; i--) {
                    if (range[i] > peak) {
                        peak = range[i];
                    }
                    var newDrawDown = 100.0 * (peak - range[i]) / peak;
                    ts.Add(domain[i], newDrawDown);
                    if (newDrawDown > maxDrawDown) {
                        maxDrawDown = newDrawDown;
                    }
                }
                drawDowns = ts;
                return drawDowns;
            }
        }

        TimeSeries diffs = null;
        public TimeSeries DailyReturns {
            get {
                if (diffs != null) return diffs;
                diffs = this.GetDiffs();
                return diffs;
            }
        }

        public ScatterSeries RankOrder(bool positive, bool negative) {
            var tograph = new List<double>();
            if (positive) {
                tograph.AddRange(range.Where(i => i > 0));
            }
            if (negative) {
                tograph.AddRange(range.Where(i => i < 0));
            }
            return range.GraphRankOrder(Name);
        }

        public string GetStats() {
            string s = string.Empty;
            if (domain.Count() == 0) return s;
            s += "Name: " + Name + "\n";
            s += "Percent change: " + PercentChange.ToString() + "\n";
            s += "Range: " + TotalChange.ToString() + "\n";
            s += "Start date: " + StartDate.ToShortDateString() + "\n";
            s += "End date: " + EndDate.ToShortDateString() + "\n";
            s += "Start val: " + StartVal.ToString() + "\n";
            s += "End val: " + EndVal.ToString() + "\n";
            return s;
        }

        public ScatterSeries ToScatterSeries(string title) {
            var ss = new ScatterSeries() { MarkerSize = 2 };
            ss.Title = title;
            ss.Points.Clear();
            foreach (var p in points) {
                ss.Points.Add(new ScatterPoint() { X = p.X, Y = p.Y });
            }
            return ss;
        }

        public LineSeries ToLineSeries(string title) {
            var ls = new LineSeries() { StrokeThickness = .5, CanTrackerInterpolatePoints = false };
            ls.Title = title;
            ls.Points.Clear();
            foreach (var p in points) {
                ls.Points.Add(p);
            }
            return ls;
        }

        private double standardDeviation = double.MinValue;
        public double StandardDeviation {
            get {
                if (standardDeviation != double.MinValue) return standardDeviation;
                double ret = 0;
                if (range.Count() > 0) {
                    //Compute the Average      
                    double avg = range.Average();
                    //Perform the Sum of (value-avg)_2_2      
                    double sum = range.Sum(d => Math.Pow(d - avg, 2));
                    //Put it all together      
                    ret = Math.Sqrt((sum) / (range.Count() - 1));
                }
                return ret;
            }
        }

        public double this[int i] {
            get { return range[i]; }
            set { range[i] = value; }
        }

        private double average = double.MinValue;
        public double Average {
            get {
                if (average == double.MinValue) {
                    return range.Average();
                } else {
                    return average;
                }
            }
        }

        public int Count() {
            return domain.Count();
        }

        public LineSeries Normalize0() {
            points.ForEach(i => {
                i.Y /= StartVal;
                i.Y -= 1;
            });
            return GetLineSeries();
        }

        public void CalculateRuns() {
            Stopwatch s1, s2, s3;
            s1 = new Stopwatch();
            s2 = new Stopwatch();
            s3 = new Stopwatch();
            s1.Restart();
            List<StraightRun> RUNS = new List<StraightRun>();
            for (int i = points.Count() - 1; i >= 0; i -= 5) {
                s2.Restart();
                for (int j = i - 1; j >= 0; j -= 5) {
                    s3.Restart();
                    double rise = points[j].Y - points[i].Y;
                    //double range = points[j].X - points[i].X;
                    double range = j - i;

                    double slope = rise / range;
                    if (Math.Abs(slope) < 3.5) continue;

                    var data = points.Skip(j).Take(i).Select(l => l.Y);

                    var sr = new StraightRun(rise, data, slope, range);
                    if (sr.Interestingness > .0004) {
                        RUNS.Add(sr);
                        Debug.Print(sr.Interestingness.ToString());
                        Debug.Print("Slope: " + slope.ToString());
                        Debug.Print("Range: " + range);
                    }
                    //Debug.Print("Loop 1: " + s3.Elapsed);
                }
                Debug.Print("Loop 2: " + s2.Elapsed);
            }
            s1.Stop();
        }

        public LineSeries Normalize1() {
            points.ForEach(i => i.Y /= StartVal);
            return GetLineSeries();
        }

        public LineSeries GetLineSeries() {
            var ls = new LineSeries() { Title = Name, StrokeThickness = .5, CanTrackerInterpolatePoints = false };
            foreach (var p in points) {
                ls.Points.Add(p);
            }
            return ls;
        }

        public void ShowLineGraph(string title = "") {
            var chart = new Chart();
            chart.AddSeries(this.GetLineSeries());
            chart.ShowUserControl();
        }
        //Take a data set
        //calculate standard deviation, draw downs, annual return, sharpe, sortino, Jensen's alpha
        //Test correlation between different time series

        public ScatterSeries CorrelateWithNextDay() {
            ScatterSeries ss = new ScatterSeries() { MarkerSize = .8, MarkerStroke = OxyColors.Blue, MarkerFill = OxyColors.Blue };
            bool axis1Direction = this.domain[1] - this.domain[0] > 0;
            int i = this.Count() - 1;
            while (i >= 1) {
                ss.Points.Add(new ScatterPoint(this.range[i], this.range[--i]));
            }
            return ss;
        }

        public ScatterSeries Correlate2(TimeSeries ts2) {
            ScatterSeries ss = new ScatterSeries() { MarkerSize = .8, MarkerStroke = OxyColors.Blue, MarkerFill = OxyColors.Blue };
            bool axis1Direction = this.domain[1] - this.domain[0] > 0;
            bool axis2Direction = ts2.domain[1] - ts2.domain[0] > 0;
            int i, j;
            if (axis1Direction) {
                i = this.Count() - 1;
            } else {
                i = 0;
            }
            if (axis2Direction) {
                j = ts2.Count() - 1;
            } else {
                j = 0;
            }

            while (i >= 0 && j >= 0 && i < this.Count() && j < ts2.Count()) {
                double domaini = this.domain[i];
                double domainj = ts2.domain[j];
                if (domaini != domainj) {
                    if (domaini > domainj) {
                        //Move i
                        if (axis1Direction) {
                            i--;
                        } else {
                            i++;
                        }
                    } else {
                        //Move j
                        if (axis2Direction) {
                            j--;
                        } else {
                            j++;
                        }
                    }
                    continue;
                } else {
                    ss.Points.Add(new ScatterPoint(this.range[i], ts2.range[j]));
                    //Move both
                    if (axis1Direction) {
                        i--;
                    } else {
                        i++;
                    }
                    if (axis2Direction) {
                        j--;
                    } else {
                        j++;
                    }
                }
            }
            return ss;
        }

        public ScatterSeries Correlate(TimeSeries ts2) {
            ScatterSeries ss = new ScatterSeries() { MarkerSize = .8, MarkerStroke = OxyColors.Blue, MarkerFill = OxyColors.Blue };
            bool axis1Direction = this.domain[1] - this.domain[0] > 0;
            bool axis2Direction = ts2.domain[1] - ts2.domain[0] > 0;
            int i = this.Count() - 1;
            int j = ts2.Count() - 1;
            while (i >= 0 && j >= 0) {
                double domaini = this.domain[i];
                double domainj = ts2.domain[j];
                if (domaini != domainj) {
                    if (domaini > domainj) {
                        if (axis1Direction) {
                            i--;
                        } else {
                            j--;
                        }
                    } else {
                        if (axis2Direction) {
                            j--;
                        } else {
                            i--;
                        }
                    }
                    continue;
                } else {
                    ss.Points.Add(new ScatterPoint(this.range[i], ts2.range[j]));
                    i--;
                    j--;
                }

            }
            return ss;
        }

        public ScatterSeries CorrelateDailyReturns(TimeSeries ts2) {
            ScatterSeries ss = new ScatterSeries() { MarkerSize = .8, MarkerStroke = OxyColors.Blue, MarkerFill = OxyColors.Blue };
            int i = this.DailyReturns.Count() - 1;
            int j = ts2.DailyReturns.Count() - 1;
            while (i >= 0 && j >= 0) {
                double domaini = this.DailyReturns.domain[i];
                double domainj = ts2.DailyReturns.domain[j];
                if (domaini != domainj) {
                    if (domaini > domainj) {
                        i--;
                    } else { j--; }
                    continue;
                } else {
                    ss.Points.Add(new ScatterPoint(this.DailyReturns.range[i], ts2.DailyReturns.range[j]));
                    i--;
                    j--;
                }

            }
            return ss;
        }

        public List<double> GetRange() {
            return range;
        }

        public List<double> GetDomain() {
            return domain;
        }
    }
}