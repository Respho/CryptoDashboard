using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace Crypto
{
    [Route("api/[controller]")]
    public class PricesController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            //
            CryptoFeed.EnsureBackground();

            //
            if (CryptoFeed.CurrentPrices == null) return new ContentResult();

            //
            return new ContentResult()
            {
                Content = getContent(),
                ContentType = "text/html",
            };
        }

        private string getContent()
        {
            //
            CryptoFeedModel model = CryptoFeed.CurrentPrices;
            //
            string rows = "";
            rows += getRow("Gemini:ETHUSD", String.Format("${0:0,0.00}", model.GeminiETHUSD));
            rows += getRow("Gemini:BTCUSD", String.Format("${0:0,0.00}", model.GeminiBTCUSD));
            rows += getRow("Luno:BTCUSD",   String.Format("${0:0,0.00}", model.LunoBTCUSD));
            rows += getRow("Luno:BTCZAR",   String.Format("R{0:n0}", model.LunoBTCZAR));
            rows += getRow("Arbi Gap",           model.ArbiPercentage.ToString("N") + " %");
            //rows += getRow("Arbi Profitability", model.ArbiProfitability.ToString("N") + " %");
            rows += getRow("Global M.Cap", Math.Round(model.GlobalMarketCapBillion, 2).ToString() + " bln");
            rows += getRow("USDZAR", model.USDZAR.ToString());
            rows += getRow("HKDZAR", model.HKDZAR.ToString());
            rows += getRow("ZARHKD", model.ZARHKD.ToString());
            //
            string status = model.Startup.ToString("MM-dd") + " #" + model.Round.ToString();
            string lastUpdate = model.LastUpdate.ToString("MM-dd HH:mm");
            TimeSpan elapsed = DateTime.Now - model.LastUpdate;
            if (elapsed.TotalSeconds > 50)
            {
                lastUpdate += "!";
            }
            rows += getRow(status, lastUpdate);

            //
            string page = "<html><head>@head</head><body>@body</body></html>";
            page = page.Replace("@head", HtmlHead);
            page = page.Replace("@body", TemplateBody);
            page = page.Replace("@rows", rows);
            page = page + "<!-- @round -->".Replace("@round", model.Round.ToString());
            return page;
        }

        private string getRow(string label, string value)
        {
            return TemplateRow.Replace("@label", label).Replace("@value", value);
        }

        const string HtmlHead = @"
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Crypto</title>
            <style type='text/css'>
            * { font-family: Helvetica, sans-serif; font-weight: lighter; font-size: 30px; color:white; }
            body { background: #444444; }
            td { border-bottom: 1px solid #999999; vertical-align: baseline; }
            td.label { font-size: 16px; width: 96px; } td.value { width: 240px; text-align: right; }
            </style>";

        const string TemplateBody = @"
            <div id=content>
                <table cellspacing=0 cellpadding=4>
                    @rows
                </table>
            </div>";

        const string TemplateRow = "<tr><td class='label'>@label</td><td class='value'>@value</td></tr>";
    }
}

