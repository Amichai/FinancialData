using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Diagnostics;

namespace Quant {
    public static class YahooStockEngine {
        //private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.historicaldata%20where%20symbol%20in%20({0})%20and%20startDate%20%3D%20%222010-03-05%22%20and%20endDate%20%3D%20%222010-03-10%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
                                
        //private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.historicaldata%20where%20symbol%20in%20({0})%20and%20startDate%20%3D%20{1}%20and%20endDate%20%3D%20{2}&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            //in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

        public static void Fetch(this ObservableCollection<Quote> quotes) {
            string symbolList = String.Join("%2C", quotes.Select(w => "%22" + w.Symbol + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);
            
            XDocument doc = XDocument.Load(url);
            Parse(quotes, doc);
        }

        public static XDocument Fetch(this Quote quote, DateTime startDate, DateTime endDate) {
            string symbol = String.Join("%2C", "%22" + quote.Symbol + "%22");
            string baseURL = @"http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.historicaldata%20where%20symbol%20in%20({0})%20and%20startDate%20%3D%20%22{1}%22%20and%20endDate%20%3D%20%22{2}%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            var date1 = startDate.ToString("yyyy-MM-dd");
            var date2 = endDate.ToString("yyyy-MM-dd");
            string url2 = string.Format(baseURL, symbol, date1, date2);
            int resultCount = 0;
            XDocument doc = null;
            for (int i = 0; i < 55; i++) {
                doc = XDocument.Load(url2);
                resultCount = doc.Elements().ElementAt(0).Elements().ElementAt(0).Elements().Count();
                if (resultCount > 0) {
                    break;
                }
            }
            if (resultCount == 0) {
                return null;
            }
            return doc;
        }

        private static void Parse(this Quote quote, XDocument doc) {
            XElement results = doc.Root.Element("results");
            XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == quote.Symbol);

            quote.Ask = GetDecimal(q.Element("Ask").Value);
            quote.Bid = GetDecimal(q.Element("Bid").Value);
            quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
            quote.BookValue = GetDecimal(q.Element("BookValue").Value);
            quote.Change = GetDecimal(q.Element("Change").Value);
            quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
            quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate") + " " + q.Element("LastTradeTime").Value);
            quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
            quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
            quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
            quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
            quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
            quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
            quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
            quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
            quote.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
            quote.Ebitda = GetDecimal(q.Element("EBITDA").Value);
            quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
            quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
            quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
            quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
            quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
            quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
            quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
            quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
            quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
            quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
            quote.Name = q.Element("Name").Value;
            quote.Open = GetDecimal(q.Element("Open").Value);
            quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
            quote.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
            quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
            quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
            quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
            quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
            quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
            quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
            quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
            quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
            quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
            quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
            quote.Volume = GetDecimal(q.Element("Volume").Value);
            quote.StockExchange = q.Element("StockExchange").Value;

            quote.LastUpdate = DateTime.Now;
        }

        private static void Parse(this ObservableCollection<Quote> quotes, XDocument doc) {
            XElement results = doc.Root.Element("results");

            foreach (Quote quote in quotes) {
                XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == quote.Symbol);

                quote.Ask = GetDecimal(q.Element("Ask").Value);
                quote.Bid = GetDecimal(q.Element("Bid").Value);
                quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
                quote.BookValue = GetDecimal(q.Element("BookValue").Value);
                quote.Change = GetDecimal(q.Element("Change").Value);
                quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
                quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate") + " " + q.Element("LastTradeTime").Value);
                quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
                quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
                quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
                quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
                quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
                quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
                quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
                quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
                quote.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
                quote.Ebitda = GetDecimal(q.Element("EBITDA").Value);
                quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
                quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
                quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
                quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
                quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
                quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
                quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
                quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
                quote.Name = q.Element("Name").Value;
                quote.Open = GetDecimal(q.Element("Open").Value);
                quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
                quote.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
                quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
                quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
                quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
                quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
                quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
                quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
                quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
                quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
                quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
                quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
                quote.Volume = GetDecimal(q.Element("Volume").Value);
                quote.StockExchange = q.Element("StockExchange").Value;

                quote.LastUpdate = DateTime.Now;
            }
        }

        private static decimal? GetDecimal(string input) {
            if (input == null) return null;

            input = input.Replace("%", "");

            decimal value;

            if (Decimal.TryParse(input, out value)) return value;
            return null;
        }

        private static DateTime? GetDateTime(string input) {
            if (input == null) return null;

            DateTime value;

            if (DateTime.TryParse(input, out value)) return value;
            return null;
        }
    }
}
