using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json.Linq;

namespace Crypto
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public string Get()
        {
            //
            CryptoFeed.EnsureBackground();

            //
            if (CryptoFeed.CurrentPrices == null) return "";
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(
                CryptoFeed.CurrentPrices, Newtonsoft.Json.Formatting.Indented);
            string formatted = Common.FormatJson(serialized);
            //
            return formatted;
        }
    }
}
