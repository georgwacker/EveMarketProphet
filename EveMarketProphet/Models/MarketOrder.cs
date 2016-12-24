using Newtonsoft.Json;
using System;

namespace EveMarketProphet.Models
{
    public class MarketOrder //: IEquatable<MarketOrder>
    {
        [JsonProperty("buy")]
        public bool Bid { get; set; }

        [JsonProperty("issued")]
        public DateTime IssueDate { get; set; }

        [JsonProperty("price")]
        public long Price { get; set; }

        [JsonProperty("volume")]
        public int VolumeRemaining { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("id")]
        public long OrderId { get; set; }

        [JsonProperty("minVolume")]
        public int MinVolume { get; set; }

        [JsonProperty("volumeEntered")]
        public int VolumeEntered { get; set; }

        [JsonProperty("range")]
        public string Range { get; set; }

        [JsonProperty("stationID")]
        public long StationId { get; set; }

        [JsonProperty("type")]
        public int TypeId { get; set; }

        public string TypeName { get; set; }
        public double TypeVolume { get; set; }
        public int RegionId { get; set; }

        public string StationName { get; set; }
        public double StationSecurity { get; set; }
        public string RegionName { get; set; }
        public int SolarSystemId { get; set; }
        public string SolarSystemName { get; set; }

        /*public bool Equals(MarketOrder other)
        {
            return OrderID.Equals(other.OrderID);
        }*/
    }
}
