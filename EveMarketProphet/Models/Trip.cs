using System.Collections.Generic;
using EveMarketProphet.Services;

namespace EveMarketProphet.Models
{
    public class Trip
    {
        public List<Transaction> Transactions { get; set; }
        public List<int> Waypoints { get; set; }
        public List<SolarSystem> SecurityWaypoints { get; set; }
        public int Jumps { get; set; }
        public double Profit { get; set; }
        public double ProfitPerJump { get; set; }
        public double Cost { get; set; }
        public double Weight { get; set; }
    }
}