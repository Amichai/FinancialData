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
using Quant;
using AvalonDock.Layout;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            this.DataContext = this;
            InitializeComponent();
            this.StartDate = "01/01/2002";
            this.EndDate = "01/01/2013";

            //A, AA, AAPL, ABC, ABI, ABT, ACE, ACS
            ///Normalize price data by dividing every data set by the first value so that all data sets are relative to /start at 1
            this.symbols.ItemsSource = data.AllSymbols();
            this.symbols.SelectionChanged += new SelectionChangedEventHandler(symbols_SelectionChanged);
            this.startDate.TextChanged += new TextChangedEventHandler(startDate_TextChanged);
            this.endDate.TextChanged += new TextChangedEventHandler(endDate_TextChanged);

            var ts = this.data.Open("AAPL", DataManager.DataType.AdjClose, DateTime.Parse(this.StartDate), DateTime.Parse(this.EndDate));
            this.addChart(ts.GetLineSeries().Graph(dateTimeAxis: this.TimeAxis), ts.Name);
        }

        DataManager data = new DataManager();

        void endDate_TextChanged(object sender, TextChangedEventArgs e) {
            if (!DateTime.TryParse(endDate.Text, out endDate_dt)) {
                this.endDate.Background = errorColor;
            } else {
                this.endDate.Background = workingColor;
            }
        }

        void startDate_TextChanged(object sender, TextChangedEventArgs e) {
            if (!DateTime.TryParse(startDate.Text, out startDate_dt)) {
                this.startDate.Background = errorColor;
            } else {
                this.startDate.Background = workingColor;
            }
        }

        void symbols_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.SearchParameter = (string)this.symbols.SelectedItem;
            this.InspectionSymbols.Clear();
            foreach (var cur in this.symbols.SelectedItems) {
                this.InspectionSymbols.Add((string)cur);
            }
        }

        private string _searchParameter;

        public string SearchParameter {
            get { return _searchParameter; }
            set {
                _searchParameter = value;
                OnPropertyChanged("SearchParameter");
            }
        }


        private void addChart(Chart ct, string title) {
            this.rightgroup.Children.Insert(0, new LayoutDocument() { Content = ct, Title = title });
            this.rightgroup.Children.First().IsSelected = true;
        }

        private ObservableCollection<string> _inspectionSymbols = new ObservableCollection<string>();

        public ObservableCollection<string> InspectionSymbols {
            get {
                return _inspectionSymbols;
            }
            set {
                _inspectionSymbols = value;
                OnPropertyChanged("InspectionSymbols");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private LineSeries getSeries(TimeSeries ts) {
            if (LogarithmicX) {
                ts.LogarithmicX();
            }
            if (LogarithmicY) {
                ts.LogarithmicY();
            }

            if (this.Normalize0) {
                return ts.Normalize0();
            } else if (this.Normalize1) {
                return ts.Normalize1();
            } else {
                return ts.GetLineSeries();
            }
        }

        private void dailyClose() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.Close, startDate_dt, endDate_dt);
                s.Add(getSeries(ts));
            }
            this.addChart(s.Graph(dateTimeAxis: this.TimeAxis), "Close");
        }

        private void adjustedClose() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts));
            }
            this.addChart(s.Graph(dateTimeAxis: this.TimeAxis), "AdjClose");
        }

        private void volume() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.Volume, startDate_dt, endDate_dt);
                s.Add(getSeries(ts));
            }
            this.addChart(s.Graph(dateTimeAxis: this.TimeAxis), "Volume");
        }

        private void dailyReturns() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts.DailyReturns));
            }
            this.addChart(s.Graph(dateTimeAxis: this.TimeAxis), "DailyReturns");
        }

        private void drawDowns1() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts.DrawDowns));
            }
            this.addChart(s.Graph(dateTimeAxis: this.TimeAxis), "DrawDowns1");
        }

        private void drawDowns2() {
            this.TimeAxis = false;
            List<ScatterSeries> s = new List<ScatterSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getDistributionSeries(ts.DrawDown2, ts.Name));
            }
            this.addChart(s.Graph(), "DrawDowns2");
        }

        private void LineChart_Click(object sender, RoutedEventArgs e) {
            switch ((string)((ContentControl)this.chartType.SelectedItem).Content) {
                case "Daily Close":
                    dailyClose();
                    break;
                case "Adjusted Close":
                    adjustedClose();
                    break;
                case "Volume":
                    volume();
                    break;
                case "Daily Returns":
                    dailyReturns();
                    break;
                case "Draw Downs 1":
                    drawDowns1();
                    break;
                case "Draw Downs 2":
                    drawDowns2();
                    break;
                default:
                    throw new Exception();
            }
        }

        private ScatterSeries getDistributionSeries(List<double> ts, string Name) {
            if (OnlyNegative && OnlyPositive) throw new Exception();
            if (OnlyPositive) {
                return ts.Where(i => i > 0).GraphRankOrder(Name, this.AbsoluteValue, this.LogarithmicY);
            } else if (OnlyNegative) {
                return ts.Where(i => i < 0).GraphRankOrder(Name, this.AbsoluteValue, this.LogarithmicY);
            }
            return ts.GraphRankOrder(Name, this.AbsoluteValue, this.LogarithmicY);
        }

        private void DailyReturnsDistribution_Click(object sender, RoutedEventArgs e) {
            List<ScatterSeries> s = new List<ScatterSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getDistributionSeries(ts.DailyReturns.GetRange(), ts.Name));
            }
            this.addChart(s.Graph(), "DrawDowns2");
        }

        private void DrawDownHist_Click(object sender, RoutedEventArgs e) {
            List<RectangleBarSeries> s = new List<RectangleBarSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                if (binSizeVal == 0) {
                    s.Add(new Histogram(ts.DrawDowns).GetBarSeries());
                } else {
                    s.Add(new Histogram(ts.DrawDowns, binSizeVal).GetBarSeries());
                }
                s.Graph().ShowUserControl();
            }
            //this.addChart(s.Graph(), "DrawDowns2");

        }

        private void DailyReturnsHist_Click(object sender, RoutedEventArgs e) {
            List<RectangleBarSeries> s = new List<RectangleBarSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                if (binSizeVal == 0) {
                    s.Add(new Histogram(ts.DailyReturns).GetBarSeries());
                } else {
                    s.Add(new Histogram(ts.DailyReturns, binSizeVal).GetBarSeries());
                }
                s.Graph().ShowUserControl();
            }
            this.addChart(s.Graph(), "DrawDowns2");
        }

        private double binSizeVal;
        private string _binSize;

        public string BinSize {
            get { return _binSize; }
            set {
                _binSize = value;
                if (!double.TryParse(this.binSize.Text, out binSizeVal)) {
                    this.binSize.Background = errorColor;
                } else {
                    this.binSize.Background = workingColor;
                }
                OnPropertyChanged("BinSize");
            }
        }


        private Brush errorColor = Brushes.LightPink;
        private Brush workingColor = Brushes.LightGreen;

        private string _startDate;

        public string StartDate {
            get { return _startDate; }
            set {
                _startDate = value;

                OnPropertyChanged("StartDate");
            }
        }

        private DateTime startDate_dt;
        private DateTime endDate_dt;

        private string _endDate;

        public string EndDate {
            get { return _endDate; }
            set {
                _endDate = value;
                OnPropertyChanged("EndDate");
            }
        }

        private bool _timeAxis = true;

        public bool TimeAxis {
            get { return _timeAxis; }
            set {
                _timeAxis = value;
                OnPropertyChanged("TimeAxis");
            }
        }

        private bool _logarithmicX = false;

        public bool LogarithmicX {
            get { return _logarithmicX; }
            set {
                _logarithmicX = value;
                OnPropertyChanged("LogarithmicX");
            }
        }

        private bool _logarithmicY = false;

        public bool LogarithmicY {
            get { return _logarithmicY; }
            set {
                _logarithmicY = value;
                OnPropertyChanged("LogarithmicY");
            }
        }

        private bool _normalize0 = false;

        public bool Normalize0 {
            get { return _normalize0; }
            set {
                if (Normalize1) Normalize1 = false;
                _normalize0 = value;
                OnPropertyChanged("Normalize0");
            }
        }

        private bool _normalize1 = false;

        public bool Normalize1 {
            get { return _normalize1; }
            set {
                if (Normalize0) Normalize0 = false;
                _normalize1 = value;
                OnPropertyChanged("Normalize1");
            }
        }

        private bool _onlyNegative;

        public bool OnlyNegative {
            get { return _onlyNegative; }
            set {
                _onlyNegative = value;
                if (_onlyNegative && _onlyPositive) {
                    OnlyPositive = false;
                }
                OnPropertyChanged("OnlyNegative");
            }
        }

        private bool _onlyPositive;

        public bool OnlyPositive {
            get { return _onlyPositive; }
            set {
                _onlyPositive = value;
                if (_onlyPositive && _onlyNegative) {
                    OnlyNegative = false;
                }
                OnPropertyChanged("OnlyPositive");
            }
        }

        private bool _absoluteValue;

        public bool AbsoluteValue {
            get { return _absoluteValue; }
            set {
                _absoluteValue = value;
                OnPropertyChanged("AbsoluteValue");
            }
        }

        private void binSize_TextChanged(object sender, TextChangedEventArgs e) {
            if (!double.TryParse(this.binSize.Text, out binSizeVal)) {
                this.binSize.Background = errorColor;
            } else {
                this.binSize.Background = workingColor;
            }
        }
    }
}
////TODO: Event analysis, correlation analysis, 2 variable scatter plots
///Give every chart an explanorty sidepanel/ legend text box
///

///TOdo: three side panes - symbol selection, data filter (adjusted close, volume, draw downs etc), chart type (line, rank order , histogram, etc)