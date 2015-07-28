using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Connexionz;
using API.Models.GoogleTransit;

namespace API.Components
{
    /// <summary>
    /// Merges data obtained from Connexionz and Google Transit and making
    /// it ready for delivery for clients.
    /// </summary>
    public static class TransitClient
    {
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
        /// The outer dictionary takes a route number and gives a dictionary that takes a stop ID to an ETA.
        /// </summary>
        public static async Task<Dictionary<string, Dictionary<string, int>>> GetEtas(List<string> stopIds)
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

        /// <summary>
        /// Fabricates a bunch of schedule information for a route on a particular day.
        /// </summary>
        /// <param name="connexionzRoute"></param>
        /// <param name="routeSchedule"></param>
        public static List<BusStopSchedule> InterpolateSchedule(ConnexionzRoute connexionzRoute, List<GoogleStopSchedule> schedule)
        {
            var adherencePoints = connexionzRoute.Path
                .Select((val, idx) => new { value = val, index = idx })
                .Where(a => a.value.IsScheduleAdherancePoint)
                .ToList();

            var results = new List<BusStopSchedule>();
            for (int i = 0; i < adherencePoints.Count - 1; i++)
            {
                int sectionLength = adherencePoints[i + 1].index - adherencePoints[i].index;
                var stopsInBetween = connexionzRoute.Path.GetRange(adherencePoints[i].index, sectionLength);
                
                var differences = schedule[i].Times.Zip(schedule[i + 1].Times,
                    (startTime, endTime) => endTime.Subtract(startTime));
                var stepSizes = differences.Select(d => d.Ticks / sectionLength);
                
                results.AddRange(stopsInBetween.Select(
                    (val, idx) => new BusStopSchedule
                    {
                        Id = val.PlatformId,
                        Times = schedule[i].Times.Zip(stepSizes, (time, step) => time.Add(TimeSpan.FromTicks(step * idx)))
                                                 .ToList()
                    }));
            }

            return results;
        }

        // todo: create appropriate return type
        public static void CreateArrivals()
        {
            var routeSchedules = GoogleTransitImport.GoogleRoutes.Value.Item2;
            var routes = ConnexionzClient.Routes.Value;
            
            foreach (var route in routes)
            {
                InterpolateSchedule(route, routeSchedules.First(rs => rs.ConnexionzName == route.RouteNo).Days.First().StopSchedules);
            }

            var platforms = ConnexionzClient.Platforms;
        }
    }
}