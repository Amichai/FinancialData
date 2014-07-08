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

namespace ChartingToolkit {
    /// <summary>
    /// Interaction logic for RankOrder.xaml
    /// </summary>
    public partial class RankOrder : ChartingBase {
        public RankOrder() {
            InitializeComponent();
        }

        public bool Abs { get; set; }
        public bool LogY { get; set; }

        public override void Draw() {
            var scatterSeries = this.Domain.GetScatterSeriesRankOrder(this.Abs, this.LogY);
            Plot = new PlotModel();
            Plot.Series.Clear();
            Plot.Series.Add(scatterSeries);
            Plot.Title = Title;
            OnPropertyChanged("Plot");
        }

        public void Add(double val) {
            throw new NotImplementedException();
        }
    }
}
