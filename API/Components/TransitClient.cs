﻿using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Connexionz;
using API.Models.GoogleTransit;

namespace API.Components
{
    /// <summary>
    /// Static client that downloads and returns deserialized route and platform details.
    /// </summary>
    public static class TransitClient
    {
        public static List<BusRoute> Routes { get; private set; }

        public static Lazy<Tuple<List<GoogleRoute>, List<GoogleRouteSchedule>>> GoogleRoutes =
            new Lazy<Tuple<List<GoogleRoute>, List<GoogleRouteSchedule>>>(GoogleTransitImport.DoTask);

        /// <summary>
        /// Performs route and stop lookups and builds the static data used in the /static route.
        /// </summary>
        public static async Task<object> GetStaticData()
        {
            var routes = await CacheManager.GetStaticRoutesAsync();
            var stops = await CacheManager.GetStaticStopsAsync();

            return new
            {
                routes = routes.ToDictionary(r => r.RouteNo),
                stops = stops.ToDictionary(s => s.ID)
            };
        }

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.  Performs the calls to get the info in parallel,
        /// aggregating the data into a dictionary.
        /// </summary>
        public static async Task<object> GetEtas(List<string> stopIds)
        {
            Dictionary<string, string> toPlatformTag = await CacheManager.GetPlatformTagsAsync();

            Func<string, Tuple<string, ConnexionzPlatformET>> getEtaIfTagExists =
                id => Tuple.Create(id, toPlatformTag.ContainsKey(id) ?
                                       CacheManager.GetEta(toPlatformTag[id]) :
                                       null);

            // If there's only one requested, it's waayyy faster to just do this serially.
            // Running the AsParallel() query below incurs significant overhead.
            if (stopIds.Count == 1)
            {
                var arrival = getEtaIfTagExists(stopIds.First());
                var dict = new Dictionary<string, Dictionary<string, int>>();

                dict.Add(arrival.Item1, // The Stop ID acting as the key for this arrival
                         arrival.Item2?.RouteEstimatedArrivals // The ETAs, transformed into a dictionary of { routeNo, ETA }.
                                        ?.ToDictionary(route => route.RouteNo,
                                                       route => route.EstimatedArrivalTime));
                return dict;
            }


            // Extracting the ETA is done as a query run in parallel such that one thread is dedicated to one ETA.
            // The results are then coalesced into a dictionary where the keys are the requested stop IDs, and the
            // values are also dictionaries.  These sub-dictionaries are a pair of { route, ETA in minutes}, where
            // the route name is the key.
            return stopIds.AsParallel()
                          .Select(getEtaIfTagExists)
                          .ToDictionary(eta => eta.Item1, // The Stop ID for this ETA
                                        eta => eta.Item2?.RouteEstimatedArrivals?.ToDictionary(route => route.RouteNo, // The dictionary of { Route Number, ETA } for the above Stop ID.
                                                                                               route => route.EstimatedArrivalTime));
        }
    }
}