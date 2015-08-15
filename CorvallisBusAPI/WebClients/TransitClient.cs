using API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models.Connexionz;
using API.Models.GoogleTransit;
using API.DataAccess;
using Newtonsoft.Json;

namespace API.WebClients
{
    using ServerBusSchedule = Dictionary<string, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// Maps a 5-digit stop ID to a dictionary that maps a route number to an arrival estimate in minutes.
    /// </summary>
    using BusArrivalEstimates = Dictionary<string, Dictionary<string, int>>;

    /// <summary>
    /// Merges data obtained from Connexionz and Google Transit
    /// and makes it ready for delivery to clients.
    /// </summary>
    public static class TransitClient
    {
        public static List<BusStop> CreateStops()
        {
            var platforms = ConnexionzClient.Platforms.Value;
            var routes = ConnexionzClient.Routes.Value;

            return platforms.Select(p => 
                    new BusStop(p, routes.Where(r => r.Path.Any(rp => rp.PlatformId == int.Parse(p.PlatformNo)))
                                         .Select(r => r.RouteNo)
                                         .ToList()))
                            .ToList();
        }

        public static List<BusRoute> CreateRoutes()
        {
            var googleRoutes = GoogleTransitClient.GoogleRoutes.Value.Item1.ToDictionary(gr => gr.ConnexionzName);
            var routes = ConnexionzClient.Routes.Value;
            return routes.Select(r => new BusRoute(r, googleRoutes)).ToList();
        }

        public static object CreateStaticData()
        {
            var routes = CreateRoutes();
            var stops = CreateStops();

            return new
            {
                routes = routes.ToDictionary(r => r.RouteNo),
                stops = stops.ToDictionary(s => s.ID)
            };
        }

        /// <summary>
        /// Lazy-loaded association between Platform Numbers (Think Stop ID) and Platform Tags (Connexionz-used value to get ETA).
        /// </summary>
        public static Dictionary<string, string> CreatePlatformTags() =>
            ConnexionzClient.Platforms.Value.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.  Performs the calls to get the info in parallel,
        /// aggregating the data into a dictionary.
        /// The outer dictionary takes a route number and gives a dictionary that takes a stop ID to an ETA.
        /// </summary>
        public static async Task<BusArrivalEstimates> GetEtas(Dictionary<string, string> toPlatformTag, IEnumerable<string> stopIds)
        {
            // TODO: make the transit client get ETAs one at a time, async, and make the manager deal with getting many ETAs.
            Func<string, Tuple<string, ConnexionzPlatformET>> getEtaIfTagExists =
                id => Tuple.Create(id, toPlatformTag.ContainsKey(id) ?
                                       ConnexionzClient.GetPlatformEta(toPlatformTag[id]).Result :
                                       null);

            // If there's only one requested, it's waayyy faster to just do this serially.
            // Running the AsParallel() query below incurs significant overhead.
            if (stopIds.Count() == 1)
            {
                var arrival = getEtaIfTagExists(stopIds.First());
                var dict = new BusArrivalEstimates();

                dict.Add(arrival.Item1, // The Stop ID acting as the key for this arrival
                         arrival.Item2?.RouteEstimatedArrivals // The ETAs, transformed into a dictionary of { routeNo, ETA }.
                                        ?.ToDictionary(route => route.RouteNo,
                                                       route => route.EstimatedArrivalTime)
                         ?? new Dictionary<string, int>());

                return dict;
            }


            // Extracting the ETA is done as a query run in parallel such that one thread is dedicated to one ETA.
            // The results are then coalesced into a dictionary where the keys are the requested stop IDs, and the
            // values are also dictionaries.  These sub-dictionaries are a pair of { route, ETA in minutes}, where
            // the route name is the key.
            return stopIds.AsParallel()
                          .Select(getEtaIfTagExists)
                          .ToDictionary(eta => eta.Item1, // The Stop ID for this ETA
                                        eta => eta.Item2?.RouteEstimatedArrivals
                                                          ?.ToDictionary(route => route.RouteNo, // The dictionary of { Route Number, ETA } for the above Stop ID.
                                                                         route => route.EstimatedArrivalTime)
                                               ?? new Dictionary<string, int>());
        }

        /// <summary>
        /// Fabricates a bunch of schedule information for a route on a particular day.
        /// </summary>
        private static List<Tuple<int, List<TimeSpan>>> InterpolateSchedule(ConnexionzRoute connexionzRoute, List<GoogleStopSchedule> schedule)
        {
            var adherencePoints = connexionzRoute.Path
                .Select((val, idx) => new { value = val, index = idx })
                .Where(a => a.value.IsScheduleAdherancePoint)
                .ToList();

            // some invariants we can rely on:
            // the first stop is an adherence point.
            // the last stop is not listed as an adherence point, even though it has a schedule in google.
            // therefore, we're going to add the last stop in manually so we can use the last schedule and interpolate.
            adherencePoints.Add(new { value = connexionzRoute.Path.Last(), index = connexionzRoute.Path.Count });
            
            var results = new List<Tuple<int, List<TimeSpan>>>();
            for (int i = 0; i < adherencePoints.Count - 1; i++)
            {
                int sectionLength = adherencePoints[i + 1].index - adherencePoints[i].index;
                var stopsInBetween = connexionzRoute.Path.GetRange(adherencePoints[i].index, sectionLength);
                
                var differences = schedule[i].Times.Zip(schedule[i + 1].Times,
                    (startTime, endTime) => endTime.Subtract(startTime));

                // we're simply going to add an even time span the schedules of consecutive stops.
                // there doesn't appear to be a more reliable way of estimating than this.
                var stepSizes = differences.Select(d => d.Ticks / sectionLength);
                
                results.AddRange(stopsInBetween.Select(
                    (val, idx) => Tuple.Create(
                        val.PlatformId,
                        schedule[i].Times.Zip(stepSizes, (time, step) => time.Add(TimeSpan.FromTicks(step * idx)))
                                         .ToList())));
            }

            return results;
        }
        
        /// <summary>
        /// Creates a bus schedule based on Google Transit data.
        /// </summary>
        public static ServerBusSchedule CreateSchedule()
        {
            var googleRouteSchedules = GoogleTransitClient.GoogleRoutes.Value.Item2.ToDictionary(schedule => schedule.ConnexionzName);
            var routes = ConnexionzClient.Routes.Value;

            // build all the schedule data for intermediate stops
            var routeSchedules = routes.Select(r => new
            {
                routeNo = r.RouteNo,
                daySchedules = googleRouteSchedules[r.RouteNo].Days.Select(
                    d => new
                    {
                        days = d.Days,
                        stopSchedules = InterpolateSchedule(r, d.StopSchedules)
                    })
            });

            // now turn it on its head so it's easy to query from a stop-oriented way.
            var platforms = ConnexionzClient.Platforms.Value;

            var result = platforms.ToDictionary(p => p.PlatformNo,
                p => routeSchedules.Select(r => new BusStopRouteSchedule
                {
                    RouteNo = r.routeNo,
                    DaySchedules = r.daySchedules.Select(ds => new BusStopRouteDaySchedule
                    {
                        Days = ds.days,
                        Times = ds.stopSchedules.FirstOrDefault(ss => ss.Item1 == int.Parse(p.PlatformNo))?.Item2
                    })
                    .Where(ds => ds.Times != null)
                    .ToList()
                })
                .Where(r => r.DaySchedules.Any())
            );

            return result;
        }
    }
}