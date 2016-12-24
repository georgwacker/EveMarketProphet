using EveMarketProphet.Models;
using JackLeitch.RateGate;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EveMarketProphet.Properties;
using EveMarketProphet.Utils;

namespace EveMarketProphet.Services
{
    public class Market
    {
        // entries for one page of market orders
        private ConcurrentBag<List<MarketOrder>> OrdersBag { get; set; } 

        public ILookup<int, MarketOrder> OrdersByType { get; private set; }

        public static Market Instance { get; } = new Market();

        public async Task FetchOrders(List<int> regions)
        {
            OrdersBag = new ConcurrentBag<List<MarketOrder>>(); //new List<MarketOrder>(3000000);

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var pageRequests = new ConcurrentQueue<string>();

            foreach (var regionId in regions)
            {
                pageRequests.Enqueue($"https://crest-tq.eveonline.com/market/{regionId}/orders/all/?page=1");
            }

            // EVE crest allows 150 requests per second
            // ignoring the burst allowance of 400 for now

            await Task.Run(() =>
            {
                using (var client = new HttpClient(handler))
                using (var rateGate = new RateGate(150, TimeSpan.FromSeconds(1)))
                {
                    ParallelUtils.ParallelWhileNotEmpty(pageRequests, (item, adder) =>
                    {
                        rateGate.WaitToProceed();

                        var result = client.GetAsync(item).Result;
                        var json = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<MarketOrderCollection>(json);

                        if (response != null)
                        {
                            if (response.next != null) adder(response.next.href);
                            OrdersBag.Add(response.Orders);
                            //Orders.AddRange(response.Orders);
                            //Trace.WriteLine("Download: " + item);
                        }
                    });
                }
            }).ContinueWith((prevTask) =>
            {
                var orders = new List<MarketOrder>(3000000);
                foreach (var list in OrdersBag)
                {
                    orders.AddRange(list);
                }

                var groupedByStation = orders.ToLookup(x => x.StationId);

                foreach (var stationGroup in groupedByStation)
                {
                    var station = Db.Instance.Stations.FirstOrDefault(s => s.stationID == stationGroup.Key);
                    if (station == null) continue;

                    foreach (var order in stationGroup)
                    {
                        order.SolarSystemId = station.solarSystemID;
                        order.RegionId = station.regionID;
                        order.StationSecurity = station.security; //SecurityUtils.RoundSecurity(station.security);
                    }
                }

                orders.RemoveAll(x => x.SolarSystemId == 0);

                if (Settings.Default.IgnoreNullSecStations)
                    orders.RemoveAll(x => x.StationSecurity <= 0.0);

                if (Settings.Default.IgnoreContraband)
                {
                    var contraband = Db.Instance.ContrabandTypes.GroupBy(x => x.typeID).Select(x => x.Key);
                    orders.RemoveAll(x => contraband.Contains(x.TypeId));
                }

                OrdersByType = orders.ToLookup(o => o.TypeId); // create subsets for each item type
            });

            

            /*await Task.Run(() =>
            {
                var groupedByStation = Orders.ToLookup(x => x.StationID);

                foreach (var stationGroup in groupedByStation)
                {
                    var station = DB.Instance.Stations.FirstOrDefault(s => s.stationID == stationGroup.Key);
                    if (station == null) continue;

                    foreach (var order in stationGroup)
                    {
                        order.SolarSystemID = station.solarSystemID;
                        order.RegionID = station.regionID;
                        order.StationSecurity = station.security;
                    }
                }

                if(Settings.Default.IgnoreNullSecStations)
                    Orders.RemoveAll(x => x.StationSecurity <= 0.0);

                if (Settings.Default.IgnoreContraband)
                {
                    var contraband = DB.Instance.ContrabandTypes.GroupBy(x => x.typeID).Select(x => x.Key);
                    Orders.RemoveAll(x => contraband.Contains(x.TypeID));
                }
            });*/

            //OrdersByType = orders.ToLookup(o => o.TypeID); // create subsets for each item type
            //Orders.Clear();
        }


        public static double GetTaxRate()
        {
            var baseTax = 0.02; // 2% base Tax
            var reduction = 0.1; // 10% reduction per skill level
            var accounting = Settings.Default.AccountingSkill;

            return baseTax - (baseTax * (accounting*reduction));
        }
    }
}
