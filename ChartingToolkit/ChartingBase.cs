using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using OxyPlot;

namespace ChartingToolkit {
    public abstract class ChartingBase : UserControl, INotifyPropertyChanged {

        public ChartingBase() {
            this.DataContext = this;
        }

        internal Dataset1d _domain;
        public Dataset1d Domain {
            get {
                return _domain;
            }
            set {
                _domain = value;
                Draw();
            }
        }

        internal Dataset1d _range;
        public Dataset1d Range {
            get {
                return _range;
            }
            set {
                _range = value;
                Draw();
            }
        }

        internal TimeSeries _timeSeries;
        public TimeSeries TimeSeries {
            get {
                return _timeSeries;
            }
            set {
                _timeSeries = value;
                Draw();
            }
        }

        public string Title { get; set; }

        public abstract void Draw();

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private PlotModel _plot = new PlotModel();

        public PlotModel Plot {
            get { return _plot; }
            set {
                _plot = value;
                OnPropertyChanged("Plot");
            }
        }
    }
}
