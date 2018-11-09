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
using Flurl;
using Flurl.Http;

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
            OrdersBag = new ConcurrentBag<List<MarketOrder>>(); 

            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var pageRequests = new ConcurrentQueue<string>();

            foreach (var regionId in regions)
            {
                var orderURL = $"https://esi.evetech.net/latest/markets/{regionId}/orders/";
                var head = await orderURL.HeadAsync();

                var pages_str = head.Headers.GetValues("x-pages").FirstOrDefault();
                var pages = Int32.Parse(pages_str);

                for (int i = 1; i <= pages; i++)
                {
                    pageRequests.Enqueue(orderURL.SetQueryParam("page", i));
                }
            }

            await Task.Run(() =>
            {
                Parallel.ForEach(pageRequests, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, (request) =>
                {
                    var result = request.GetJsonAsync<List<MarketOrder>>().Result;
                    if(result != null)
                    {
                        OrdersBag.Add(result);
                    }
                });

                /*using (var client = new HttpClient(handler))
                using (var rateGate = new RateGate(150, TimeSpan.FromSeconds(1)))
                {
                    ParallelUtils.ParallelWhileNotEmpty(pageRequests, (item, adder) =>
                    {
                        //rateGate.WaitToProceed();

                        var result = client.GetAsync(item).Result;

                        var json = result.Content.ReadAsStringAsync().Result;
                        var response = JsonConvert.DeserializeObject<List<MarketOrder>>(json);

                        if (response != null)
                        {
                            //if (response.next != null) adder(response.next.href);
                                OrdersBag.Add(response);
                            
                            //Orders.AddRange(response.Orders);
                            //Trace.WriteLine("Download: " + item);
                        }
                    });
                }*/
            }).ContinueWith((prevTask) =>
            {
                var orders = new List<MarketOrder>(3000000);
                foreach (var list in OrdersBag)
                {
                    orders.AddRange(list);
                }
                OrdersBag = null;
                //var orders = OrdersBag.ToList();

                var groupedByStation = orders.ToLookup(x => x.StationId);

                foreach (var stationGroup in groupedByStation)
                {
                    var station = Db.Instance.Stations.FirstOrDefault(s => s.stationID == stationGroup.Key);
                    if (station == null) continue;

                    foreach (var order in stationGroup)
                    {
                        //order.SolarSystemId = station.solarSystemID;
                        order.RegionId = station.regionID;
                        order.StationSecurity = station.security; //SecurityUtils.RoundSecurity(station.security);
                    }
                }

                orders.RemoveAll(x => x.SystemId == 0);

                if (Settings.Default.IgnoreNullSecStations)
                    orders.RemoveAll(x => x.StationSecurity <= 0.0);

                if (Settings.Default.IgnoreContraband)
                {
                    var contraband = Db.Instance.ContrabandTypes.GroupBy(x => x.typeID).Select(x => x.Key);
                    orders.RemoveAll(x => contraband.Contains(x.TypeId));
                }

                OrdersByType = orders.ToLookup(o => o.TypeId); // create subsets for each item type
                orders = null;
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
