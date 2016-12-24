using System;
using System.Collections.Generic;
using EveMarketProphet.Services;

namespace EveMarketProphet.Models
{
    public class Transaction
    {
        public MarketOrder BuyOrder { get; private set; }
        public MarketOrder SellOrder { get; private set; }
        public double Profit { get; private set; }
        public int Quantity { get; private set; }

        public long StartStationId => SellOrder.StationId; 
        public long EndStationId => BuyOrder.StationId;
        public int StartSystemId => SellOrder.SolarSystemId;
        public int EndSystemId => BuyOrder.SolarSystemId;
        public int TypeId => SellOrder.TypeId;
        public string TypeName => SellOrder.TypeName;
        public double TypeVolume => SellOrder.TypeVolume;
        public string Icon => $"https://image.eveonline.com/Type/{TypeId}_64.png";

        public long Cost => Quantity * SellOrder.Price;
        public double Weight => Quantity * SellOrder.TypeVolume;

        public List<int> Waypoints { get; set; }
        public double ProfitPerJump { get; set; } //{ get { return Waypoints.Count > 0 ? Profit / Waypoints.Count : Profit; } }
        public int Jumps { get; set; } //=> ( Waypoints != null ? Waypoints.Count : 0 );

        public Transaction(MarketOrder sell, MarketOrder buy)
        {
            SellOrder = sell;
            BuyOrder = buy;

            Quantity = Math.Min(buy.VolumeRemaining, sell.VolumeRemaining);
            //Quantity = sell.VolumeRemaining > buy.VolumeRemaining ? buy.VolumeRemaining : sell.VolumeRemaining;
            var buyPrice = sell.Price * Quantity;
            var sellPrice = buy.Price * Quantity;
            var tax = Market.GetTaxRate();

            Profit = sellPrice - (sellPrice * tax) - buyPrice;
        }

        public Transaction(MarketOrder sell, MarketOrder buy, int quantity)
        {
            SellOrder = sell;
            BuyOrder = buy;
            Quantity = quantity;

            var buyPrice = sell.Price * quantity;
            var sellPrice = buy.Price * quantity;
            var tax = Market.GetTaxRate();

            Profit = sellPrice - (sellPrice * tax) - buyPrice;
        }
    }
}