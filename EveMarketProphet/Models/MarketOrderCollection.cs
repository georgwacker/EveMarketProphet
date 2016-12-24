using Newtonsoft.Json;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace EveMarketProphet.Models
{
    [UsedImplicitly]
    public class Paginator
    {
        public string href { get; set; }
    }

    [UsedImplicitly]
    public class MarketOrderCollection
    {
        [JsonProperty("items")]
        public List<MarketOrder> Orders { get; set; }
        public int totalCount { get; set; }
        public Paginator next { get; set; }
        public int pageCount { get; set; }
    }
}
