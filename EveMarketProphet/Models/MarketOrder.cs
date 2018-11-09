using JetBrains.Annotations;
using Newtonsoft.Json;
using System;

namespace EveMarketProphet.Models
{
    [UsedImplicitly]
    public class MarketOrder //: IEquatable<MarketOrder>
    {
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("is_buy_order")]
        public bool Bid { get; set; }

        [JsonProperty("issued")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("location_id")] 
        public long StationId { get; set; } // https://docs.esi.evetech.net/docs/asset_location_id

        [JsonProperty("min_volume")]
        public int MinVolume { get; set; }

        [JsonProperty("order_id")]
        public long OrderId { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("range")]
        public string Range { get; set; }

        [JsonProperty("system_id")] // new in ESI?
        public int SystemId { get; set; }

        [JsonProperty("type_id")]
        public int TypeId { get; set; }

        [JsonProperty("volume_remain")]
        public int VolumeRemaining { get; set; }

        [JsonProperty("volume_total")]
        public int VolumeEntered { get; set; } //TODO: refactor


        public string TypeName { get; set; }
        public double TypeVolume { get; set; }
        public int RegionId { get; set; }

        public string StationName { get; set; }
        public double StationSecurity { get; set; }
        public string RegionName { get; set; }
        //public int SolarSystemId { get; set; }
        public string SolarSystemName { get; set; }

        /*public bool Equals(MarketOrder other)
        {
            return OrderID.Equals(other.OrderID);
        }*/
    }
}
