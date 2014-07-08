using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using System.ComponentModel;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace ChartingToolkit {
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : ChartingBase {
        public void Add(List<double> vals) {
            foreach (var a in vals) {
                Add(a);
            }
        }

        public Histogram() {
            InitializeComponent();
            //this.DataContext = this;
            this.BinSize = 0;
        }

        private double _binSize;

        public double BinSize {
            get { return _binSize; }
            set { 
                _binSize = value;
                OnPropertyChanged("BinSize");
            }
        }

        private int _numberOfDataPoints;

        public int NumberOfDataPoints {
            get { return _numberOfDataPoints; }
            set {
                _numberOfDataPoints = value;
                OnPropertyChanged("NumberOfDataPoints");
            }
        }

        public void Add(double val) {
            this.Domain.Add(val);
            NumberOfDataPoints++;
            int binIdx = (int)Math.Floor(val / this.BinSize);
            var columnVals = (List<double>)this.series.Tag;
            for (int i = 0; i < columnVals.Count; i++) {
                if (val > columnVals[i] && val < columnVals[i] + this.BinSize) {
                    series.Items.Where(j => j.CategoryIndex == i).SingleOrDefault().Value++;
                } else {
                    Draw();
                }
            }
        }
        
        private ColumnSeries series;

        public override void Draw() {
            if (this.NumberOfDataPoints == 0) {
                this.NumberOfDataPoints = _domain.Count();
            }
            if (this.BinSize == 0) {
                BinSize = Math.Round(3.49 * _domain.StandardDev() * Math.Pow(_domain.Count(), -.33333), 3);
            }
            this.series = Domain.GetColumnSeries(this.BinSize);
            Plot = new PlotModel();
            var ca = new CategoryAxis() {
                LabelField = "label",
            };
            Plot.Axes.Add(ca);
            ((CategoryAxis)Plot.Axes[0]).Labels = ((IList<double>)series.Tag).Select(i => i.ToString()).ToList();
            Plot.Series.Clear();
            Plot.Series.Add(series);
            Plot.Title = Title;
            OnPropertyChanged("Plot");
        }

        private void redraw_Click(object sender, RoutedEventArgs e) {
            Draw();
        }
    }
}
