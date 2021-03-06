﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using OxyPlot;
using System.IO;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using OxyPlot.Series;
using OxyPlot.Axes;

namespace Quant {
    public class DataManager {

        string directory = @"C:\Users\Amichai\Data\QSData\QSData\Yahoo";
        string[] files;

        Dictionary<string, int> symbolIndex;

        public IEnumerable<TimeSeries> AllAvailable(int max, DateTime start, DateTime end) {
            for(int i=0; i< files.Count(); i++){
                if (max != -1 && i > max) break;
                //adjusted close
                yield return Open(i, start, end, 6);
            }
        }

        public DataManager() {

            symbolIndex = new Dictionary<string, int>();
            
            files = Directory.GetFiles(directory);
            int counter = 0;
            foreach (var filepath in files) {
                symbolIndex[getTitle(counter)] = counter++;
            }

        }

        public enum DataType { AdjClose, Volume, Close, Open, High, Low }

        public void GetOpenCloseHighLow(string symbl, DateTime start, DateTime end, out TimeSeries open, out TimeSeries close, out TimeSeries high, out TimeSeries low) {
            var a = new Quote(symbl);
            XDocument doc = null;
            if (Properties.Settings.Default.PollWebAPI) {
                doc = a.Fetch(start.Date, end);
                if (doc == null) {
                    throw new Exception();
                }
            }

            open = new TimeSeries(symbl);
            close = new TimeSeries(symbl);
            high = new TimeSeries(symbl);
            low = new TimeSeries(symbl);

            if (!Properties.Settings.Default.PollWebAPI) {
                open = Open(symbl, start, end, 1);
                high = Open(symbl, start, end, 2);
                low = Open(symbl, start, end, 3);
                close = Open(symbl, start, end, 4);
                return;
            }


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

        private DateTime start, end;
        public TimeSeries Open(string symbl, DataType type, DateTime start, DateTime end) {
            this.start = start;
            this.end = end;
            symbl = symbl.ToUpper();
            var a = new Quote(symbl);
            //var doc = a.Fetch(DateTime.Parse("2010-03-5").Date, DateTime.Parse("2011-03-5"));

            TimeSeries ts = new TimeSeries(symbl);
            if (type == DataType.AdjClose) {
                return Open(symbolIndex[symbl], start, end, 6);
            }

            if (type == DataType.Volume) {
                return Open(symbolIndex[symbl], start, end, 5);
            }

            if (type == DataType.Close) {
                return Open(symbolIndex[symbl], start, end, 4);
            }
            //if (type == DataType.DailyReturns) {
            //    return DailyReturns(symbolIndex[symbl], start, end);
            //}
            throw new Exception();
        }

        public TimeSeries Open2(string symbl, DataType type, DateTime start, DateTime end) {
            var a = new Quote(symbl);
            XDocument doc = null;
            if (Properties.Settings.Default.PollWebAPI) {
                doc = a.Fetch(start.Date, end);
                if (doc == null) {
                    throw new Exception();
                }
            }

            TimeSeries ts = new TimeSeries(symbl);
            if (type == DataType.AdjClose) {
                if (!Properties.Settings.Default.PollWebAPI) {
                    return Open(symbl, start, end, 6);
                }
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Adj_Close").Value));
                }
            }

            if (type == DataType.Volume) {
                if (!Properties.Settings.Default.PollWebAPI) {
                    return Open(symbl, start, end, 5);
                }
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Volume").Value));
                }
            }

            if (type == DataType.Close) {
                if (!Properties.Settings.Default.PollWebAPI) {
                    return Open(symbl, start, end, 4);
                }
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Close").Value));
                }
            }

            if (type == DataType.Open) {
                if (!Properties.Settings.Default.PollWebAPI) {
                    return Open(symbl, start, end, 1);
                }
                foreach (var quote in doc.Descendants("quote")) {
                    ts.Add(DateTimeAxis.ToDouble(DateTime.Parse(quote.Element("Date").Value)),
                        double.Parse(quote.Element("Open").Value));
                }
            }

            if (type == DataType.High) {
                if (!Properties.Settings.Default.PollWebAPI) {
                    return Open(symbl, start, end, 2);
                }
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

        public TimeSeries DailyReturns(int i, DateTime start, DateTime end) {
            var r = getReader(i);
            TimeSeries ts = new TimeSeries(getTitle(i));
            double lastVal = double.NaN;
            foreach (var p in r.readData(0, 6, start, end)) {
                double y = p.Y / lastVal - 1;
                if (lastVal == double.NaN) y = 0;
                ts.Add(p.X, y);
                lastVal = p.Y;
            }
            return ts;
        }

        private CsvReader getReader(string symbol) {
            for (int i = 0; i < files.Count(); i++) {
                if (getTitle(i).ToUpper() == symbol.ToUpper()) {
                    return getReader(i);
                }
            }
            throw new Exception("symbol not found");
        }

        private CsvReader getReader(int i) {
            string filepath = files[i];
            var a = File.OpenText(filepath);
            return new CsvReader(a);
        }

        private string getTitle(int i) {
            string filepath = files[i];
            var titleTemp = filepath.Split('\\').Last();
            return titleTemp.Split('.').First();
        }

        private void format(LineSeries ls, int i) {
            ls.StrokeThickness = .5;
            ls.Title = getTitle(i);
        }

        public TimeSeries Open(string symbol, DateTime start, DateTime end, int fieldIdx = 6) {
            TimeSeries ts = new TimeSeries(symbol);
            var r = getReader(symbol);
            try {
                foreach (var p in r.readData(0, fieldIdx, start, end)) {
                    ts.Add(p.X, p.Y);
                }
            } catch {

            }
            return ts;
        }

        public TimeSeries Open(int i, DateTime start, DateTime end, int fieldIdx = 6) {
            TimeSeries ts = new TimeSeries(getTitle(i));
            var r = getReader(i);
            try {
                foreach (var p in r.readData(0, fieldIdx, start, end)) {
                    ts.Add(p.X, p.Y);
                }
            }
            catch{

            }
            return ts;
        }

        public IEnumerable<string> AllSymbols() {
            for(int i=0;i < files.Count(); i++){
                yield return getTitle(i);
            }
        }

        private string urlBase = @"http://download.finance.yahoo.com/d/quotes.csv?s=";


    }
}
