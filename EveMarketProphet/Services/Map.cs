using System.Collections.Generic;
using System.Linq;
using QuickGraph;
using QuickGraph.Algorithms;
using System.Collections.Concurrent;

namespace EveMarketProphet.Services
{
    public class Map
    {
        private AdjacencyGraph<int, Edge<int>> Graph { get; set; }
        private AdjacencyGraph<int, Edge<int>> GraphHighSec { get; set; }
        private ConcurrentDictionary<int, TryFunc<int, IEnumerable<Edge<int>>>> FunctionCache { get; set; }
        private ConcurrentDictionary<int, TryFunc<int, IEnumerable<Edge<int>>>> FunctionCacheHighSec { get; set; }

        public static Map Instance { get; } = new Map();

        private Map()
        {
            Graph = CreateGraph(false);
            GraphHighSec = CreateGraph(true);
            FunctionCache = new ConcurrentDictionary<int, TryFunc<int, IEnumerable<Edge<int>>>>();
            FunctionCacheHighSec = new ConcurrentDictionary<int, TryFunc<int, IEnumerable<Edge<int>>>>();
        }

        private AdjacencyGraph<int, Edge<int>> CreateGraph(bool isHighSec)
        {
            var security = isHighSec ? 0.5 : -10;
            var graph = new AdjacencyGraph<int, Edge<int>>(false);
            
            var edges = from e in Db.Instance.SolarSystemJumps
                        join system1 in Db.Instance.SolarSystems on e.fromSolarSystemID equals system1.solarSystemID
                        join system2 in Db.Instance.SolarSystems on e.toSolarSystemID equals system2.solarSystemID
                        where system1.security >= security && system2.security >= security
                        select new Edge<int>(e.fromSolarSystemID, e.toSolarSystemID);

            var vertices = Db.Instance.SolarSystems.Where(x => x.security >= security).Select(x => x.solarSystemID);

            graph.AddVertexRange(vertices.ToList());
            graph.AddEdgeRange(edges.ToList());

            return graph;
        }

        public List<int> FindRoute(int startSolarSystemId, int endSolarSystemId)
        {
            var route = new List<int>();
            var isHighSec = Properties.Settings.Default.IsHighSec;

            // return empty list for inner-solarsystem trade
            if (startSolarSystemId == endSolarSystemId) return route;

            var graph = isHighSec ? GraphHighSec : Graph;
            var cache = isHighSec ? FunctionCacheHighSec : FunctionCache;

            // starting system is not part of the graph
            if (!graph.ContainsVertex(startSolarSystemId)) return null;

            TryFunc<int, IEnumerable<Edge<int>>> funcPath;
            IEnumerable<Edge<int>> path;

            if (cache.ContainsKey(startSolarSystemId))
            {
                funcPath = cache[startSolarSystemId];
            }
            else
            {
                funcPath = graph.ShortestPathsDijkstra(e => 1, startSolarSystemId);
                cache[startSolarSystemId] = funcPath;
            }

            if(funcPath(endSolarSystemId, out path))
            {
                foreach (var p in path)
                    route.Add(p.Target);
            }

            // destination is unreachable
            if (route.Count == 0) return null;

            return route;
        }

    }
}
