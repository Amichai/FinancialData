﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot;
using CsvHelper;
using System.Collections.ObjectModel;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Quant {
    public static class Util {
        public static void ShowUserControl(this UserControl control) {
            Window window = new Window {
                Title = "Chart",
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Content = control
            };

            window.ShowDialog();
        }

        public static double StandardDev(this IEnumerable<double> range) {
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

        public static ScatterSeries GraphRankOrder(this IEnumerable<double> pts, string name, 
            bool abs = true, bool logy = false) {
            List<double> pts2;
            if (abs) {
                pts2 = pts.OrderByDescending(i => Math.Abs(i)).ToList();
            } else {
                pts2 = pts.OrderByDescending(i => i).ToList();
            }
            ScatterSeries ls = new ScatterSeries() { Title = name };
            
            for (int i = 0; i < pts2.Count(); i++) {
                double val = pts2[i];
                if (abs) {
                    val = Math.Abs(val);
                }
                if (logy) {
                    val = Math.Log(val);
                }
                ls.Points.Add(new ScatterPoint(i, val));
            }
            ls.MarkerSize = 1;
            ls.MarkerFill = OxyColors.Red;
            return ls;
        }

        public static Chart Graph(this LineSeries series, bool dateTimeAxis = false, Axis xAxis = null, Axis yAxis = null) {
            Chart chart = new Chart(dateTimeAxis, xAxis, yAxis);
            chart.AddSeries(series);
            return chart;
        }

        public static Chart Graph(this List<CandleStickSeries> series, DateTimeAxis a1, LinearAxis a2) {
            Chart chart = new Chart(true);
            foreach (var s in series) {
                chart.AddSeries(s);
            }
            return chart;
        }

        public static Chart Graph(this CandleStickSeries series, DateTimeAxis a1, LinearAxis a2) {
            Chart chart = new Chart(true);
            chart.AddSeries(series);
            return chart;
        }

        public static Chart Graph(this ScatterSeries series, Axis xAxis = null, Axis yAxis = null) {
            Chart chart = new Chart(false, xAxis, yAxis);
            chart.AddSeries(series);
            return chart;
        }

        public static Chart Graph(this List<RectangleBarSeries> series) {
            Chart chart = new Chart(false);
            foreach (var a in series) {
                chart.AddSeries(a);
            }
            return chart;
        }

        public static Chart Graph(this List<ScatterSeries> series, bool dateTimeAxis = false, Axis xAxis = null, Axis yAxis = null) {
            Chart chart = new Chart(dateTimeAxis, xAxis, yAxis);
            foreach (var a in series) {
                chart.AddSeries(a);
            }
            return chart;         
        }

        public static Chart Graph(this List<LineSeries> series, bool dateTimeAxis = true, Axis xAxis = null, Axis yAxis = null) {
            Chart chart = new Chart(dateTimeAxis, xAxis, yAxis);
            foreach (var a in series) {
                chart.AddSeries(a);
            }
            return chart;
        }

        public static IEnumerable<DataPoint> readData(this CsvReader r, int idx1, int idx2, DateTime start, DateTime end) {
            while (r.Read()) {
                var date = DateTime.Parse(r.GetField(idx1));
                if (date < start) continue;
                if (date > end) continue;
                var val = r.GetField(idx2).Parse();
                if (double.IsNaN(val)) continue;
                yield return new DataPoint(DateTimeAxis.ToDouble(date), val);
            }
        }

        public static string ToCsvRow(params object[] objects) {
            string s1 = string.Empty;
            int numberOfObjects = objects.Count();
            for (int i = 0; i < numberOfObjects; i++) {
                s1 += "{" + i + "}" + ", ";
            }
            s1 += "\n";
            return string.Format(s1, objects);
        }

        public static double Parse(this string s) {
            if (s == "nan") return double.NaN;
            return double.Parse(s);
        }

        public static OxyPlot.OxyColor ToOxyColor(this Color c) {
            return OxyPlot.OxyColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable) {
            var col = new ObservableCollection<T>();
            foreach (var cur in enumerable) {
                col.Add(cur);
            }
            return col;
        }
    }

    ///Normalize price data by dividing every data set by the first value so that all data sets are relative to /start at 1
}
