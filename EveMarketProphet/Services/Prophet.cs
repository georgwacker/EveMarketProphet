using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using EveMarketProphet.Models;
using EveMarketProphet.Properties;

namespace EveMarketProphet.Services
{
    public static class Prophet
    {
        public static List<Trip> FindTradeRoutes()
        {
            if (Market.Instance.OrdersByType == null) return null;
            if (Market.Instance.OrdersByType.Count == 0) return null;

            var profitableTx = new ConcurrentBag<Transaction>();

            // for each item type
            Parallel.ForEach(Market.Instance.OrdersByType, (orderGroup, state) =>
            //foreach(var orderGroup in Market.Instance.OrdersByType)
            {
                var orders = orderGroup.ToList();

                // query volume and name for this item type once
                var type = Db.Instance.Types.Where(t => t.typeID == orderGroup.Key).Select(t => new { typeVolume = t.volume, typeName = t.typeName }).FirstOrDefault();

                // although typeId should return a result every time, an outdated static database may yield null
                if (type == null) return;

                var volume = type.typeVolume;
                var name = type.typeName;

                foreach (var o in orders)
                {
                    o.TypeVolume = volume;
                    o.TypeName = name;
                }

                var transactions = from s in orders
                                   join b in orders on s.TypeId equals b.TypeId
                                   where s.Bid == false && b.Bid == true && s.Price < b.Price
                                   //select new { sell = s, buy = b };
                                   select new Transaction(s, b);

                foreach (var tx in transactions)
                {
                    //var tx = new Transaction(t.sell, t.buy);

                    if (Settings.Default.IsHighSec && tx.SellOrder.StationSecurity < 0.5) continue;
                    if (tx.Profit < Settings.Default.MinBaseProfit) continue;

                    tx.Waypoints = Map.Instance.FindRoute(tx.StartSystemId, tx.EndSystemId);
                    if (tx.Waypoints == null) continue;

                    tx.Jumps = tx.Waypoints.Count;
                    tx.ProfitPerJump = tx.Jumps > 0 ? tx.Profit / (double)tx.Jumps : tx.Profit;

                    if (tx.ProfitPerJump >= Settings.Default.MinProfitPerJump)
                        profitableTx.Add(tx);

                    //profitableTx.Add(tx);
                }

            }); // Parallel.ForEach

            if (profitableTx.Count == 0) return null;

            var playerSystemId = Auth.Instance.GetLocation();
            if (playerSystemId == 0) playerSystemId = Settings.Default.DefaultLocation;

            var stationPairGroups = profitableTx.GroupBy(x => new { x.StartStationId, x.EndStationId }).
                                                            Select(y => y.OrderByDescending(x => x.Profit).ToList());

            var trips = new List<Trip>();

            Parallel.ForEach(stationPairGroups, tx =>
            //foreach(var tx in stationPairGroups)
            {
                var selectedTx = new List<Transaction>();

                // find the route once for this station pair
                var waypoints = tx.First().Waypoints; //Map.FindRoute(tx.First().StartSystemId, tx.First().EndSystemId);

                var isk = Settings.Default.Capital;
                var vol = Settings.Default.MaxCargo;

                var types = tx.GroupBy(x => x.TypeId).Select(x => x.Key);

                foreach(var typeId in types)
                {
                    var buyOrders = tx.Where(x => x.TypeId == typeId).Select(x => x.BuyOrder).OrderByDescending(x => x.Price).GroupBy(x => x.OrderId).Select(x => x.First()).ToList();
                    var sellOrders = tx.Where(x => x.TypeId == typeId).Select(x => x.SellOrder).OrderBy(x => x.Price).GroupBy(x => x.OrderId).Select(x => x.First()).ToList();

                    var tracker = sellOrders.GroupBy(x => x.OrderId).Select(x => x.First()).ToDictionary(x => x.OrderId, x => x.VolumeRemaining);

                    // try to fill buy orders
                    foreach (var buyOrder in buyOrders)
                    {
                        var quantityToFill = Math.Max(buyOrder.MinVolume, buyOrder.VolumeRemaining);
                        var temp = new List<Transaction>();

                        foreach (var sellOrder in sellOrders)
                        {
                            if (tracker[sellOrder.OrderId] <= 0) continue;
                            if (sellOrder.Price <= 0) continue;

                            var quantity = Math.Min(tracker[sellOrder.OrderId], quantityToFill);
                            var partialTx = new Transaction(sellOrder, buyOrder, quantity);
                            partialTx.Waypoints = waypoints;
                            partialTx.Jumps = waypoints.Count;
                            partialTx.ProfitPerJump = partialTx.Jumps > 0 ? partialTx.Profit / (double)partialTx.Jumps : partialTx.Profit;

                            if (partialTx.Cost <= isk && partialTx.Weight <= vol)
                            {
                                quantityToFill -= partialTx.Quantity;
                                isk -= partialTx.Cost;
                                vol -= partialTx.Weight;
                                temp.Add(partialTx);
                            }
                            else
                            {
                                //add partially
                                var quantityIsk = (int)(isk/sellOrder.Price);
                                var quantityWeight = (int)(vol/sellOrder.TypeVolume);
                                var quantityPart = Math.Min(quantityIsk, quantityWeight);

                                if (quantityPart > 0)
                                {
                                    var partTx = new Transaction(sellOrder, buyOrder, quantityPart);
                                    partTx.Waypoints = waypoints;
                                    partTx.Jumps = waypoints.Count;
                                    partTx.ProfitPerJump = partTx.Jumps > 0
                                        ? partTx.Profit/(double) partTx.Jumps
                                        : partTx.Profit;

                                    if (partTx.Profit > Settings.Default.MinFillerProfit)
                                    {
                                        quantityToFill -= partTx.Quantity;
                                        isk -= partTx.Cost;
                                        vol -= partTx.Weight;
                                        temp.Add(partTx);
                                    }
                                }
                            }

                            // Filled buy order
                            if (quantityToFill == 0) break;

                        } // foreach sell order

                        // Filling the buy order was successfull, commit 
                        if (quantityToFill == 0 || quantityToFill > buyOrder.MinVolume)
                        {
                            selectedTx.AddRange(temp);

                            foreach (var t in temp)
                            {
                                //isk -= t.Cost;
                                //vol -= t.Weight;
                                tracker[t.SellOrder.OrderId] -= t.Quantity;
                                //t.Waypoints = waypoints;
                            }
                        }
                        else // could not fill  buy order completely, undo
                        {
                            foreach(var t in temp)
                            {
                                isk += t.Cost;
                                vol += t.Weight;
                            }
                        }
                    } // foreach buy order

                } // for each type

                if(selectedTx.Count > 0)
                {
                    // creating a trip out of transactions for this station pair
                    var trip = new Trip();
                    trip.Transactions = selectedTx;
                    trip.Profit = trip.Transactions.Sum(x => x.Profit);
                    trip.Cost = trip.Transactions.Sum(x => x.Cost);
                    trip.Weight = trip.Transactions.Sum(x => x.Weight);

                    // change playerSystemId here to find remote routes
                    trip.Waypoints = Map.Instance.FindRoute(playerSystemId, trip.Transactions.First().StartSystemId);

                    if (trip.Waypoints != null)
                    {
                        trip.Jumps = trip.Waypoints.Count + trip.Transactions.First().Jumps;
                        trip.ProfitPerJump = (trip.Jumps > 0 ? trip.Profit / (double)trip.Jumps : trip.Profit);

                        var ssn = Db.Instance.Stations.First(s => s.stationID == trip.Transactions.First().StartStationId).stationName;
                        var bsn = Db.Instance.Stations.First(s => s.stationID == trip.Transactions.First().EndStationId).stationName;

                        // TODO: just save the station names once in the trip class
                        foreach (var t in trip.Transactions)
                        {
                            t.SellOrder.StationName = ssn;
                            t.BuyOrder.StationName = bsn;
                        }

                        trip.SecurityWaypoints = new List<SolarSystem>();

                        foreach (var systemId in trip.Waypoints.Concat(trip.Transactions.First().Waypoints))
                        {
                            var sec = Db.Instance.SolarSystems.First(x => x.solarSystemID == systemId);
                            trip.SecurityWaypoints.Add(sec);
                        }

                        if (trip.ProfitPerJump >= Settings.Default.MinProfitPerJump)
                            trips.Add(trip);
                    }
                }

            });

            return trips.OrderByDescending(x => x.ProfitPerJump).ToList();
        }
    }
}
