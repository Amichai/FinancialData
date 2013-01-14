using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CsvHelper;
using OxyPlot;
using System.Diagnostics;

namespace Quant {
    class Program {
        [STAThread]
        static void Main(string[] args) {
            //DateTime end = DateTime.Now.Subtract(TimeSpan.FromDays(0));
            DateTime end = DateTime.Parse("01/01/2013");
            DateTime start = DateTime.Parse("01/01/2002");
            var data = new DataManager();
            //A, AA, AAPL, ABC, ABI, ABT, ACE, ACS
            ///Normalize price data by dividing every data set by the first value so that all data sets are relative to /start at 1

            //var dwj= data.Open(0,6);
            ////dwj.ShowLineGraph();
            var AAPL = data.Open(3, 3);
            AAPL.DrawDown2.Where(i => i > 0).GraphRankOrder("AAPL").Graph();
            AAPL.DrawDown2.Where(i => i < 0).GraphRankOrder("AAPL").Graph();
            AAPL.DailyReturns.RankOrder(true, false).Graph();
            AAPL.DailyReturns.RankOrder(false, true).Graph();

                //var dd2 = dd.OrderByDescending(i => Math.Abs(i));
            
            //var s1 = AAPL.DailyReturns.GetLineSeries();
            //var s2 = dwj.DailyReturns.GetLineSeries();
            //dwj.CorrelateDailyReturns(AAPL).Graph();
            ///Hig
            return;
            string filepath = "output.csv";
            StreamWriter writer = new StreamWriter(filepath, append: false);
            writer.WriteLine("Name,StartDate,EndDate,StartVal,EndVal,Annual Return,PercentChange,Daily Return StandardDev,Average Daily Return,Max, Min, Max Draw Down");
            foreach (var close in data.AllAvailable(2)) {
                List<LineSeries> series = new List<LineSeries>();
                //var b = new Histogram(a.DrawDowns);
                //b.ShowGraph();
                //close.CalculateRuns();
                ///calculate the correlation between an asset and a market

                series.Add(close.GetLineSeries());
                //series.Add(close.DailyReturns.GetLineSeries());
                series.Add(close.DrawDowns.GetLineSeries());
                series.Graph();
                //a.ShowLineGraph("Close");
                //a.DailyReturns.ShowLineGraph("Daily returns");
                //a.DrawDowns.ShowLineGraph("Draw downs");
                if (close.Count() == 0) continue;
                writer.Write(Util.ToCsvRow(close.Name, close.StartDate.ToShortDateString(), close.EndDate.ToShortDateString(), close.StartVal, close.EndVal, close.TotalChange, 
                    close.PercentChange, close.DailyReturns.StandardDeviation, close.DailyReturns.Average, close.MaxVal, close.MinVal, close.MaxDrawDown));
            } 
            //Get average daily return
            //Do standard deviation of daily returns
            writer.Close();
        }

        ///Todo: draw downs
        ///Correlations
        ///Stock summary data/metrics
        ///Bar charts
        ///Api to get financial data
        ///http://digitalpbk.com/stock/google-finance-get-stock-quote-realtime
    }
}