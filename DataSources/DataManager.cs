using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ChartingToolkit;

namespace Quant {
    public class DataSources {

        string directory = @"C:\Users\Amichai\Data\QSData\QSData\Yahoo";
        string[] files;

        Dictionary<string, int> symbolIndex;

        /*
        public IEnumerable<TimeSeries> AllAvailable(int max, DateTime start, DateTime end) {
            for(int i=0; i< files.Count(); i++){
                if (max != -1 && i > max) break;
                //adjusted close
                yield return Open(i, start, end, 6);
            }
        }*/

        public DataSources() {

            symbolIndex = new Dictionary<string, int>();
            
            files = Directory.GetFiles(directory);
            int counter = 0;
            foreach (var filepath in files) {
                symbolIndex[getTitle(counter)] = counter++;
            }

        }

        public enum DataType { AdjClose, Volume, Close, Open, High, Low }
        /*
        public void GetOpenCloseHighLow(string symbl, DateTime start, DateTime end, out TimeSeries open, out TimeSeries close, out TimeSeries high, out TimeSeries low) {
            var a = new Quote(symbl);
            var doc = a.Fetch(start.Date, end);
            if (doc == null) {
                throw new Exception();
            }

            open = new TimeSeries();
            close = new TimeSeries();
            high = new TimeSeries();
            low = new TimeSeries();
            foreach (var quote in doc.Descendants("quote")) {
                open.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                    double.Parse(quote.Element("Open").Value));
                close.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                    double.Parse(quote.Element("Close").Value));
                high.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                    double.Parse(quote.Element("High").Value));
                low.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                    double.Parse(quote.Element("Low").Value));
            }
            
        }

        public TimeSeries Open(string symbl, DataType type, DateTime start, DateTime end) {
            var a = new Quote(symbl);
            var doc = a.Fetch(start.Date, end);
            if (doc == null) {
                throw new Exception();
            }

            TimeSeries ts = new TimeSeries();
            if (type == DataType.AdjClose) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Adj_Close").Value));
                }
            }

            if (type == DataType.Volume) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Volume").Value));
                }
            }

            if (type == DataType.Close) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Close").Value));
                }
            }

            if (type == DataType.Open) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Open").Value));
                }
            }

            if (type == DataType.High) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("High").Value));
                }
            }

            if (type == DataType.Low) {
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Low").Value));
                }
            }
            return ts;
        }
        */

        /*
        public TimeSeries DailyReturns(int i, DateTime start, DateTime end) {
            var r = getReader(i);
            TimeSeries ts = new TimeSeries();
            double lastVal = double.NaN;
            foreach (var p in r.readData(0, 6, start, end)) {
                double y = p.Y / lastVal - 1;
                if (lastVal == double.NaN) y = 0;
                ts.Add(p.X, y);
                lastVal = p.Y;
            }
            return ts;
        }
        */
        //private CsvReader getReader(int i) {
        //    string filepath = files[i];
        //    var a = File.OpenText(filepath);
        //    return new CsvReader(a);
        //}

        private string getTitle(int i) {
            string filepath = files[i];
            var titleTemp = filepath.Split('\\').Last();
            return titleTemp.Split('.').First();
        }
        /*
        public TimeSeries Open(int i, DateTime start, DateTime end, int fieldIdx = 6) {
            TimeSeries ts = new TimeSeries();
            var r = getReader(i);
            try {
                foreach (var p in r.readData(0, fieldIdx, start, end)) {
                    ts.Add(p.X, p.Y);
                }
            }
            catch{

            }
            return ts;
        }*/

        public IEnumerable<string> AllSymbols() {
            for(int i=0;i < files.Count(); i++){
                yield return getTitle(i);
            }
        }

        private string urlBase = @"http://download.finance.yahoo.com/d/quotes.csv?s=";


    }
}
