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
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;

namespace ChartingToolkit {
    /// <summary>
    /// Interaction logic for LineChart.xaml
    /// </summary>
    public partial class LineChart : ChartingBase {
        public LineChart() {
            InitializeComponent();
        }

        public override void Draw() {
            LineSeries series = this.TimeSeries.GetLineSeries(Title);
            Plot = new PlotModel();
            Plot.Series.Clear();
            Plot.Series.Add(series);
            Plot.Title = Title;
            OnPropertyChanged("Plot");
        }

        public void Add(DateTime x, double y) {
            this.TimeSeries.Add(x, y);
        }
    }
}
