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
using OxyPlot.Series;
using OxyPlot.Axes;

namespace UI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            this.DataContext = this;
            InitializeComponent();

            this.StartDate = "01/01/2003";
            this.EndDate = DateTime.Now.Date.ToShortDateString();

            //A, AA, AAPL, ABC, ABI, ABT, ACE, ACS
            ///Normalize price data by dividing every data set by the first value so that all data sets are relative to /start at 1
            this.symbols.ItemsSource = data.AllSymbols();
            this.symbols.SelectionChanged += new SelectionChangedEventHandler(symbols_SelectionChanged);
            this.startDate.TextChanged += new TextChangedEventHandler(startDate_TextChanged);
            this.endDate.TextChanged += new TextChangedEventHandler(endDate_TextChanged);

            var ts = this.data.Open("AAPL", DataManager.DataType.AdjClose, DateTime.Parse(this.StartDate), DateTime.Parse(this.EndDate));

            this.addChart(ts.GetLineSeries().Graph(this.TimeAxis,
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Adjusted Close " }
                ), ts.Name);
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


        private void addChart(UserControl ct, string title) {
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
            this.addChart(s.Graph(this.TimeAxis,
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Close"}), "Close");
        }

        private void adjustedClose() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts));
            }
            this.addChart(s.Graph(this.TimeAxis,
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Adjusted Close"}
                ), "AdjClose");
        }

        private void volume() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.Volume, startDate_dt, endDate_dt);
                s.Add(getSeries(ts));
            }
            this.addChart(s.Graph(this.TimeAxis,
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Volume"}), "Volume");
        }

        private void dailyReturns() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts.DailyReturns));
            }
            this.addChart(s.Graph(this.TimeAxis,
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Daily Returns" }), "DailyReturns");
        }

        private void drawDowns1() {
            List<LineSeries> s = new List<LineSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getSeries(ts.DrawDowns));
            }
            this.addChart(s.Graph(this.TimeAxis, 
                new DateTimeAxis() { Title = "Date" },
                new LinearAxis() { Title = "Draw Down"}), "DrawDowns1");
        }

        private void drawDowns2() {
            this.TimeAxis = false;
            List<ScatterSeries> s = new List<ScatterSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                s.Add(getDistributionSeries(ts.DrawDown2, ts.Name));
            }
            this.addChart(s.Graph(false, 
                new LinearAxis() { Title = "Rank" },
                new LinearAxis() { Title = "Draw Down"}), "DrawDowns2");
        }

        private void CandleSticks_Click(object sender, RoutedEventArgs e) {
            List<CandleStickSeries> all = new List<CandleStickSeries>();
            TimeSeries open, close, high, low;
            foreach (var a in InspectionSymbols) {
                this.data.GetOpenCloseHighLow(a, startDate_dt, endDate_dt, out open, out close, out high, out low);
            
                CandleStickSeries css = new CandleStickSeries() { StrokeThickness = .5, 
                    Title = "Candle sticks" };
                css.Items.Clear();
                for (int i = 0; i < open.Count(); i++) {
                    css.Items.Add(
                        new HighLowItem() {
                            Open = open.GetRange()[i],
                            Close = close.GetRange()[i],
                            High = high.GetRange()[i],
                            Low = low.GetRange()[i],
                            X = open.GetDomain()[i]
                        }
                        );
                }

                all.Add(css);
            }
                this.addChart(all.Graph(
                    new DateTimeAxis() { Title = "Date" },
                    new LinearAxis() { Title = "Close" }), "Close");
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

        #region Histograms
        ///TODO: get us multiple stacks of bars 
        private void dailyClose_hist(){
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.Close, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.GetRange(), ts.Name + " Daily Close"), ts.Name + " daily close histogram");
            }
        }

        
        private void volume_hist() {
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.Volume, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.GetRange(), ts.Name + " Volume"), ts.Name + " volume histogram");
            }
        }

        private void dailyReturns_hist() {
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.DailyReturns.GetRange(), ts.Name + " Daily Returns"), "Histogram");
            }
        }

        private void drawDowns1_hist() {
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.DrawDowns.GetRange(), ts.Name + " Draw downs 1"), "Histogram");
            }
        }

        private void drawDowns2_hist() {
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.DrawDown2, ts.Name + " Draw downs 2"), "Histogram");
            }
        }   

        private void Hist_Click(object sender, RoutedEventArgs e) {
            string dataType = (string)((ContentControl)this.chartType.SelectedItem).Content;
            if(dataType == null) return;
            switch (dataType) {
                case "Daily Close":
                    dailyClose_hist();
                    break;
                case "Adjusted Close":
                    adjustedClose_hist();
                    break;
                case "Volume":
                    volume_hist();
                    break;
                case "Daily Returns":
                    dailyReturns_hist();
                    break;
                case "Draw Downs 1":
                    drawDowns1_hist();
                    break;
                case "Draw Downs 2":
                    drawDowns2_hist();
                    break;
                default:
                    throw new Exception();
            }
        }

        private void adjustedClose_hist() {
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);
                addChart(new ChartingHelper.Histogram(ts.GetRange(), ts.Name), "Histogram");
            }
        }

        private void DailyReturnsHist_Click(object sender, RoutedEventArgs e) {
            List<RectangleBarSeries> s = new List<RectangleBarSeries>();
            foreach (var a in InspectionSymbols) {
                var ts = this.data.Open(a, DataManager.DataType.AdjClose, startDate_dt, endDate_dt);

                s.Add(new Histogram(ts.DailyReturns).GetBarSeries());
                
                s.Graph().ShowUserControl();
            }
            this.addChart(s.Graph(), "DrawDowns2");
        }
        #endregion

        private TimeSeries getTimeSeries(string symbol, string tsType){
            switch (tsType) {
                case "Daily Close":
                    DataManager.DataType t = DataManager.DataType.Close;
                    return this.data.Open(symbol, t, startDate_dt, endDate_dt);
                case "Adjusted Close":
                    t = DataManager.DataType.AdjClose;
                    return this.data.Open(symbol, t, startDate_dt, endDate_dt);
                case "Volume":
                    t = DataManager.DataType.Volume;
                    return this.data.Open(symbol, t, startDate_dt, endDate_dt);
                case "Daily Returns":
                    t = DataManager.DataType.AdjClose;
                    return this.data.Open(symbol, t, startDate_dt, endDate_dt).DailyReturns;
                case "Draw Downs 1":
                    t = DataManager.DataType.AdjClose;
                    return this.data.Open(symbol, t, startDate_dt, endDate_dt).DrawDowns;
                //case "Draw Downs 2":

                //    break;
                default:
                    throw new Exception();
            }
        }

        private void ScatterPlot_Click(object sender, RoutedEventArgs e) {
            if (InspectionSymbols.Count() < 2 && chartType.SelectedItems.Count < 2) return;
            //if (InspectionSymbols.Count() < 2) return;

            DataManager.DataType t = DataManager.DataType.AdjClose;
            TimeSeries ts1, ts2;
            var selectedItems = this.chartType.SelectedItems;

            string dataType1 = ((ListViewItem)selectedItems[0]).Content as string;
            string dataType2 = null;
            if (selectedItems.Count > 1) {
                dataType2 = ((ListViewItem)selectedItems[1]).Content as string;    
            }
            
            switch (dataType1) {
                case "Daily Close":
                    t = DataManager.DataType.Close;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    //ts2 = this.data.Open(InspectionSymbols[1], t, startDate_dt, endDate_dt);
                    break;
                case "Adjusted Close":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    //ts2 = this.data.Open(InspectionSymbols[1], t, startDate_dt, endDate_dt);
                    break;
                case "Volume":
                    t = DataManager.DataType.Volume;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    //ts2 = this.data.Open(InspectionSymbols[1], t, startDate_dt, endDate_dt);
                    break;
                case "Daily Returns":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt).DailyReturns;
                    //ts2 = this.data.Open(InspectionSymbols[1], t, startDate_dt, endDate_dt).DailyReturns;
                    break;
                case "Draw Downs 1":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt).DrawDowns;
                    //ts2 = this.data.Open(InspectionSymbols[1], t, startDate_dt, endDate_dt).DrawDowns;
                    break;
                case "Draw Downs 2":
                    return;
                default:
                    throw new Exception();
            }
            if (dataType2 != null) {
                ts2 = getTimeSeries(InspectionSymbols[0], dataType2);
            } else {
                ts2 = getTimeSeries(InspectionSymbols[1], dataType1);
            }

            this.addChart(ts1.Correlate2(ts2).Graph(new LinearAxis() { Title = ts1.Name },
                new LinearAxis() { Title = ts2.Name }),
                ts1.Name + " " + ts2.Name);
        }

        private void DayToDayCorrelation_Click(object sender, RoutedEventArgs e) {
            DataManager.DataType t = DataManager.DataType.AdjClose;
            TimeSeries ts1;
            switch ((string)((ContentControl)this.chartType.SelectedItem).Content) {
                case "Daily Close":
                    t = DataManager.DataType.Close;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    break;
                case "Adjusted Close":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    break;
                case "Volume":
                    t = DataManager.DataType.Volume;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt);
                    break;
                case "Daily Returns":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt).DailyReturns;
                    break;
                case "Draw Downs 1":
                    t = DataManager.DataType.AdjClose;
                    ts1 = this.data.Open(InspectionSymbols[0], t, startDate_dt, endDate_dt).DrawDowns;
                    break;
                case "Draw Downs 2":
                    return;
                default:
                    throw new Exception();
            }

            this.addChart(ts1.CorrelateWithNextDay().Graph(new LinearAxis() { Title = ts1.Name },
                new LinearAxis() { Title = "Next day" }),
                ts1.Name + " " + ts1.Name);
        }

        #region Properties
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
        #endregion

    }
}
////TODO: Event analysis, correlation analysis, 2 variable scatter plots
///Give every chart an explanorty sidepanel/ legend text box
///

///TOdo: three side panes - symbol selection, data filter (adjusted close, volume, draw downs etc), chart type (line, rank order , histogram, etc)
///Correlation function - corr