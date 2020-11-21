using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

using Amazon.CloudWatch.Model;

namespace Crypto
{
    public class CryptoFeedModel
    {
        public double GlobalMarketCapBillion;

        public double GeminiBTCUSD;
        public double GeminiETHUSD;
        public double LunoBTCZAR;
        public double LunoBTCUSD;
        public double KrakenBTCUSD;
        public double KrakenETHUSD;

        public double KrakenLTCUSD;
        public double KrakenDASHUSD;
        public double KrakenXMRUSD;
        public double KrakenZECUSD;

        public double KrakenXRPUSD;
        public double KrakenXLMUSD;

        public double KrakenEOSUSD;
        public double KrakenADAUSD;
        public double KrakenXTZUSD;

        public double BinanceTRXUSD;
        public double BinanceNEOUSD;
        public double BinanceQTUMUSD;

        public double BinanceNANOUSD;
        public double BinanceIOTUSD;
        public double BinanceRVNBTC;
        public double BinanceRVNUSD;

        public double USDZAR;
        public double HKDZAR;
        public double ZARHKD;

        public double ArbiPercentage;
        public double ArbiProfitability;
        public string Comments;
        public int Round = 0;
        public DateTime LastUpdate;
        public DateTime Startup = DateTime.Now;

        public void UpdateProfitability()
        {
            const double PriceBase = 6000;
            const double PriceTarget = 18000;
            const double ExchangeTarget = 1.8;
            DateTime Deadline = DateTime.Parse("2018/05/31");
            Comments = "";
            //
            LastUpdate = DateTime.Now;
            LastUpdate.AddTicks( - (LastUpdate.Ticks % TimeSpan.TicksPerSecond));
            GlobalMarketCapBillion = Math.Round(GlobalMarketCapBillion, 2);
            //
            LunoBTCUSD = Math.Round(LunoBTCZAR / USDZAR, 2);
            ArbiPercentage = (LunoBTCUSD - GeminiBTCUSD) / GeminiBTCUSD * 100;
            ArbiPercentage = Math.Round(ArbiPercentage, 4);
            //Price
            double upside = (PriceTarget / PriceBase) - (GeminiBTCUSD / PriceBase);
            Comments += "Upside: " + Math.Round(upside, 4) + " ";
            //Exchange Rate
            double exchangeFactor = HKDZAR / ExchangeTarget;
            Comments += "Exchange factor: " + Math.Round(exchangeFactor, 4) + " ";
            //Time
            TimeSpan timeSpan = Deadline - DateTime.Today;
            int daysLeft = timeSpan.Days;
            double timeFactor = 1.0 * daysLeft / 110 + 0.5;
            Comments += "Days Left: " + daysLeft + " ";
            Comments += "Time factor: " + Math.Round(timeFactor, 4) + " ";
            //
            ArbiProfitability = ArbiPercentage * upside * exchangeFactor * timeFactor;
            ArbiProfitability = Math.Round(ArbiProfitability, 4);
            //
            USDZAR = Math.Round(USDZAR, 4);
            HKDZAR = Math.Round(HKDZAR, 4);
            ZARHKD = Math.Round(ZARHKD, 4);
            //
            Round++;
        }
    }

    public class ServiceInterval
    {
        public int DefaultIntervalSeconds = 1 * 60;
        public int DefaultRetryIntervalSeconds = 10;
        public int MaxRetryIntervalSeconds = 3600;
        public bool PreviousResult = true;

        private int currentRetryIntervalSeconds;

        public ServiceInterval(int defaultIntervalSeconds)
        {
            DefaultIntervalSeconds = defaultIntervalSeconds;
        }

        public void RegisterResult(bool success)
        {
            if (!success)
            {
                if (PreviousResult)
                {
                    //First fail
                    currentRetryIntervalSeconds = DefaultRetryIntervalSeconds;
                }
                else
                {
                    //Repeated fail
                    currentRetryIntervalSeconds *= 2;
                    if (currentRetryIntervalSeconds > MaxRetryIntervalSeconds)
                    {
                        currentRetryIntervalSeconds = MaxRetryIntervalSeconds;
                    }
                }
            }
            //
            PreviousResult = success;
        }

        public void SleepInterval()
        {
            int sleep = DefaultIntervalSeconds * 1000;
            if (!PreviousResult)
            {
                sleep = currentRetryIntervalSeconds * 1000;
            }
            //Console.WriteLine("Sleep Interval " + sleep + " ms");
            System.Threading.Thread.Sleep(sleep);
        }
    }

    public class CryptoFeed
    {
        public static CryptoFeedModel CurrentPrices = null;

        static System.Threading.Thread _thread = null;
        static ServiceInterval Interval = new ServiceInterval(150);
        static bool Run = true;
        static int RoundNumber = 0;

        public static void EnsureBackground()
        {
            if (_thread != null) return;
            //
            _thread = Common.StartThread(EntryPoint);
        }

        public static void EntryPoint()
        {
            while (Run)
            {
                try
                {
                    update();
                    push();
                    Interval.RegisterResult(true);
                }
                catch (Exception e)
                {
                    Interval.RegisterResult(false);
                    Console.WriteLine(e.Message);
                }
                Interval.SleepInterval();
            }
        }

        private static void update()
        {
            //
            if (CurrentPrices == null) CurrentPrices = new CryptoFeedModel();
            //
            bool chance = new Random().Next(0, 9) > 7;
            if (chance || (CurrentPrices.GlobalMarketCapBillion < 10))
            {
                CurrentPrices.GlobalMarketCapBillion = getMarketCap();
            }
            //
            JObject jObject = getCryptowatch();
            //Gateway
            CurrentPrices.GeminiBTCUSD = getCryptowatchPrice(jObject, "market:gemini:btcusd");
            CurrentPrices.GeminiETHUSD = getCryptowatchPrice(jObject, "market:gemini:ethusd");
            CurrentPrices.KrakenBTCUSD = getCryptowatchPrice(jObject, "market:kraken:btcusd");
            CurrentPrices.KrakenETHUSD = getCryptowatchPrice(jObject, "market:kraken:ethusd");
            CurrentPrices.LunoBTCZAR = getCryptowatchPrice(jObject, "market:luno:btczar");
            //
            CurrentPrices.KrakenXRPUSD = getCryptowatchPrice(jObject, "market:kraken:xrpusd");
            CurrentPrices.KrakenXLMUSD = getCryptowatchPrice(jObject, "market:kraken:xlmusd");
            //Cash
            CurrentPrices.KrakenLTCUSD = getCryptowatchPrice(jObject, "market:kraken:ltcusd");
            CurrentPrices.KrakenDASHUSD = getCryptowatchPrice(jObject, "market:kraken:dashusd");
            CurrentPrices.KrakenXMRUSD = getCryptowatchPrice(jObject, "market:kraken:xmrusd");
            CurrentPrices.KrakenZECUSD = getCryptowatchPrice(jObject, "market:kraken:zecusd");
            //Contract Platforms
            CurrentPrices.KrakenEOSUSD = getCryptowatchPrice(jObject, "market:kraken:eosusd");
            CurrentPrices.KrakenADAUSD = getCryptowatchPrice(jObject, "market:kraken:adausd");
            CurrentPrices.KrakenXTZUSD = getCryptowatchPrice(jObject, "market:kraken:xtzusd");
            CurrentPrices.BinanceTRXUSD = getCryptowatchPrice(jObject, "market:binance:trxusdt");
            CurrentPrices.BinanceNEOUSD = getCryptowatchPrice(jObject, "market:binance:neousdt");
            CurrentPrices.BinanceQTUMUSD = getCryptowatchPrice(jObject, "market:binance:qtumusdt");
            //Block Lattice
            CurrentPrices.BinanceNANOUSD = getCryptowatchPrice(jObject, "market:binance:nanousdt");
            CurrentPrices.BinanceIOTUSD = getCryptowatchPrice(jObject, "market:binance:iotausdt");
            //
            CurrentPrices.BinanceRVNBTC = getCryptowatchPrice(jObject, "market:binance:rvnbtc");
            CurrentPrices.BinanceRVNUSD = CurrentPrices.BinanceRVNBTC * CurrentPrices.GeminiBTCUSD;
            //
            if (RoundNumber % 96 == 0)
            {
                //Dictionary<string, double> xe = getXe();
                Dictionary<string, double> xe = getOER();
                CurrentPrices.USDZAR = xe["ZAR"];
                CurrentPrices.ZARHKD = xe["HKD"] / xe["ZAR"];
                CurrentPrices.HKDZAR = 1.0 / CurrentPrices.ZARHKD;
            }
            //
            RoundNumber++;
            //
            CurrentPrices.UpdateProfitability();
        }

        private static Newtonsoft.Json.Linq.JObject getCryptowatch()
        {
            string url = "https://api.cryptowat.ch/markets/prices";
            string response = Common.WebRequest(url, Common.DefaultUserAgent, 20000);

            //
            JObject jObject = JObject.Parse(response);
            return jObject;
        }

        private static double getCryptowatchPrice(JObject jObject, string node)
        {
            JToken jNode = jObject.SelectToken("result").SelectToken(node);
            return double.Parse(jNode.ToString());
            /*{
            "result":{
                "index:cryptofacilities:cf-in-bchusd":282.32,
                "index:cryptofacilities:cf-in-ltcusd":79.49,
                "index:cryptofacilities:cf-in-xrpusd":0.32624,
                "index:cryptofacilities:cf-in-xrpxbt":0.00006391,
                "index:cryptofacilities:cme-cf-brti":5104,
                "index:cryptofacilities:cme-cf-ethusd-rti":165.42,
                "market:binance:adabtc":0.00001629,
                "market:binance:adaeth":0.00050218,
                "market:luno:btczar":76278,
                "market:luno:ethbtc":0.0326,
                "market:gemini:btcusd":5106.64,
                "market:gemini:ethbtc":0.03243,
                "market:gemini:ethusd":165.45"
                market:binance:nanousdt
                market:bitfinex:eosusd	5.399
                market:kraken:adausd	0.0832
            },
            "allowance":{
                "cost":2123102,
                "remaining":7997876898
            }*/
        }

        private static double getMarketCap()
        {
            const string API_KEY = "xxxxx";
            //
            var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/global-metrics/quotes/latest");
            var client = new System.Net.WebClient();
            client.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
            client.Headers.Add("Accepts", "application/json");
            string response = client.DownloadString(URL.ToString());

            string segment = response.Substring(response.IndexOf("total_market_cap") + 18).Split('.')[0];
            double mcap = double.Parse(segment) / 1000000000;
            return mcap;

            //{"bitcoin_percentage_of_market_cap": 35.322565413329386, "active_cryptocurrencies": 1530, "total_volume_usd": 23904891136.748413, "active_markets": 8791, "total_market_cap_by_available_supply_usd": 472316737923.8346}
            //"quote":{"USD":{"total_market_cap":458776490803.37695
            /*
            JObject jObject = JObject.Parse(response);
            double mcap = double.Parse(
                jObject.SelectToken("data")
                    .SelectToken("quote")
                    .SelectToken("usd")
                    .SelectToken("total_market_cap").ToString()) / 1000000000;
            return mcap;
            */
        }

        private static Dictionary<string, double> getOER()
        {
            //a8d244ab1cc74de9a75f7ad66cfa4a28
            //https://openexchangerates.org/api/latest.json?app_id=xxxxx
            string url = "https://openexchangerates.org/api/latest.json?app_id=xxxxx";
            string response = Common.WebRequest(url, Common.DefaultUserAgent, 10000);
            //
            JObject jObject = JObject.Parse(response);
            //
            Dictionary<string, double> rates = new Dictionary<string, double>();
            rates.Add("HKD", double.Parse(jObject.SelectToken("rates").SelectToken("HKD").ToString()));
            rates.Add("ZAR", double.Parse(jObject.SelectToken("rates").SelectToken("ZAR").ToString()));
            return rates;
        }

        static void push()
        {
            List<MetricDatum> metrics = new List<MetricDatum>();

            metrics.Add(new MetricDatum
            {
                StatisticValues = new StatisticSet(),
                MetricName = "Global M.Cap Bln USD",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.GlobalMarketCapBillion
            });

            //Exchange Kraken
            //Exchange Luno
            //Pair BTCUSD
            //Pair ETHBTC
            //Pair BTCZAR
            //Pair ETHBTC
            //Metric Price
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Gemini"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "BTCUSD"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.GeminiBTCUSD
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Gemini"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "ETHUSD"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.GeminiETHUSD
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Kraken"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "BTCUSD"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.KrakenBTCUSD
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Kraken"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "ETHUSD"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.KrakenETHUSD
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Luno"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "BTCZAR"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.LunoBTCZAR
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Exchange", Value = "Luno"
                    },
                    new Dimension()
                    {
                        Name = "Pair", Value = "BTCUSD"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Price",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.LunoBTCUSD
            });

            //Exchange Rate USDZAR
            //Exchange Rate USDHKD
            //Exchange Rate HKDZAR
            //Exchange Rate ZARHKD
            //Metric Rate
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "XE", Value = "USDZAR"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Rate",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.USDZAR
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "XE", Value = "HKDZAR"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Rate",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.None,
                Value = CurrentPrices.HKDZAR
            });

            //Arbi - Gemini Luno
            //Gap & Profitability
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Path", Value = "Gemini Luno"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Gap",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.Percent,
                Value = CurrentPrices.ArbiPercentage
            });
            metrics.Add(new MetricDatum
            {
                Dimensions = new List<Dimension>()
                {
                    new Dimension()
                    {
                        Name = "Path", Value = "Gemini Luno"
                    }
                },
                StatisticValues = new StatisticSet(),
                MetricName = "Profitability",
                Timestamp = DateTime.Now,
                Unit = Amazon.CloudWatch.StandardUnit.Percent,
                Value = CurrentPrices.ArbiProfitability
            });

            string ns = "Crypto";
            CloudWatch.PushMetrics(ns, metrics);
        }
    }
}
