using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using OxyPlot;
using System.IO;
using System.Diagnostics;

namespace Quant {
    public class DataManager {

        string directory = @"C:\Users\Amichai\Data\QSData\QSData\Yahoo";
        string[] files;

        Dictionary<string, int> symbolIndex;
        DateTime start;
        DateTime end;

        public IEnumerable<TimeSeries> AllAvailable(int max = -1) {
            for(int i=0; i< files.Count(); i++){
                if (max != -1 && i > max) break;
                //adjusted close
                yield return Open(i, 6);
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

        public enum DataType { AdjClose, DailyReturns, Volume, Close }

        public TimeSeries Open(string symbl, DataType type, DateTime start, DateTime end) {
            this.start = start;
            this.end = end;
            symbl = symbl.ToUpper();
            if (type == DataType.AdjClose) {
                return Open(symbolIndex[symbl], 6);
            }
            if (type == DataType.Volume) {
                return Open(symbolIndex[symbl], 5);
            }
            if (type == DataType.Close) {
                return Open(symbolIndex[symbl], 4);
            }
            if (type == DataType.DailyReturns) {
                return DailyReturns(symbolIndex[symbl]);
            }
            throw new Exception();
        }

        public TimeSeries DailyReturns(int i) {
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

        public TimeSeries Open(int i, int fieldIdx = 6) {
            TimeSeries ts = new TimeSeries(getTitle(i));
            var r = getReader(i);
            try {
                foreach (var p in r.readData(0, fieldIdx, start, end)) {
                    ts.Add(p.X, p.Y);
                }
            }
            catch  (Exception Exception){

            }
            return ts;
        }

        public IEnumerable<string> AllSymbols() {
            for(int i=0;i < files.Count(); i++){
                yield return getTitle(i);
            }
        }
    }
}
