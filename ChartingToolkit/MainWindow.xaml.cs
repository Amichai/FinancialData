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
using System.Threading;

namespace ChartingToolkit {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Random rand = new Random();
        public MainWindow() {
            InitializeComponent();
            //Histogram h = new Histogram();
            ////RankOrder h = new RankOrder();
            //Dataset1d d = new Dataset1d() { 20, 21, 2.2, 2.3, 3.333, 3.5, 13, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 };
            //h.Domain = d;
            //h.Draw();

            LineChart h = new LineChart();
            TimeSeries ts = new TimeSeries();
            for (int i = 0; i < 30; i++) {
                ts.Add(DateTime.Now, rand.Next(5) * rand.NextDouble());
                Thread.Sleep(1000);
            }
            h.TimeSeries = ts;
            this.Content = h;
        }
    }
}
