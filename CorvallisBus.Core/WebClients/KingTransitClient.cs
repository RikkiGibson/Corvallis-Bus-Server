using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CorvallisBus.Core.WebClients
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    public class KingTransitClient : ITransitClient
    {
        public Task<ConnexionzPlatformET?> GetEta(int platformTag)
        {
            // TODO
            return Task.FromResult<ConnexionzPlatformET?>(null);
        }

        public Task<List<ServiceAlert>> GetServiceAlerts()
        {
            // TODO
            return Task.FromResult(new List<ServiceAlert>());
        }

        private class RouteNoComparer : IEqualityComparer<BusRoute>
        {
            public bool Equals(BusRoute x, BusRoute y)
            {
                // todo: I bet there is a better way to narrow these down.
                // maybe select the one with the latest effective date that is earlier than "now"?
                return x.RouteNo == y.RouteNo;
            }

            public int GetHashCode(BusRoute obj)
            {
                return obj.RouteNo.GetHashCode();
            }

            private RouteNoComparer() { }
            public static RouteNoComparer Instance { get; } = new RouteNoComparer();
        }

        public (BusSystemData data, List<string> errors) LoadTransitData()
        {
            var stream = new HttpClient().GetStreamAsync("https://metro.kingcounty.gov/GTFS/google_transit.zip").Result;
            using var archive = new ZipArchive(stream);
            var googleData = GoogleTransitClient.LoadData(archive);

            // TODO: routes have no colors. what should be done?

            foreach (var grouping in googleData.Routes.GroupBy(r => r.RouteNo))
            {
                var fst = grouping.FirstOrDefault()?.Path;
                if (fst == null)
                    continue;

                foreach (var route in grouping)
                {
                    if (!Enumerable.SequenceEqual(route.Path, fst))
                    {
                        Console.WriteLine("meaningful duplicate found!");
                    }
                }
            }

            var routes = googleData.Routes
                .Select(gr => new BusRoute(gr.RouteNo, gr.Path, gr.Color, gr.Url, ConnexionzRoute.EncodePolyline(gr.Shape)))
                .Distinct(RouteNoComparer.Instance)
                .ToDictionary(r => r.RouteNo);

            var stops = googleData.Stops
                .Select(stop =>
                    new BusStop(
                        stop.PlatformTag,
                        stop.Name,
                        bearing: 0.0 /* TODO */,
                        stop.Lat,
                        stop.Lon,
                        routeNames: routes.Values
                            .Where(r => r.Path.Contains(stop.PlatformTag))
                            .Select(r => r.RouteNo)
                            .ToList()))
                .ToDictionary(s => s.Id);

            var staticData = new BusStaticData(routes, stops);

            var schedule = CreateSchedule(googleData.Schedules, googleData.Routes, googleData.Stops);

            // this is ugly, but basically there's no lookup to actually do.
            // TODO: where should this lookup table thing move to?
            var platformIdToPlatformTag = stops.ToDictionary(kvp => kvp.Key, kvp => kvp.Key);

            var systemData = new BusSystemData(staticData, schedule, platformIdToPlatformTag);

            // TODO: validation?
            return (systemData, new List<string>());
        }

        /// <summary>
        /// Creates a bus schedule based on Google Transit data.
        /// </summary>
        public ServerBusSchedule CreateSchedule(
            List<GoogleRouteSchedule> googleSchedules,
            List<GoogleRoute> googleRoutes,
            List<GoogleStop> googleStops)
        {
            var googleSchedulesDict = googleSchedules.ToDictionary(schedule => schedule.RouteNo);
            var routes = googleRoutes.Where(r => googleSchedulesDict.ContainsKey(r.RouteNo));

            var routeSchedules = routes.Select(r => new
            {
                routeNo = r.RouteNo,
                daySchedules = googleSchedulesDict[r.RouteNo].Days.Select(
                    d => new
                    {
                        days = d.Days,
                        stopSchedules = d.StopSchedules.Zip(r.Path, (ss, stopId) => (stopId, ss.Times))
                    })
            });

            // Now turn it on its head so it's easy to query from a stop-oriented way.
            var result = googleStops.ToDictionary(p => p.PlatformTag,
                // TODO: change type to List?
                p => routeSchedules.Select(r => new BusStopRouteSchedule(
                    routeNo: r.routeNo,
                    daySchedules: r.daySchedules.Select(ds => new BusStopRouteDaySchedule(
                        days: ds.days,
                        times: ds.stopSchedules.FirstOrDefault(ss => ss.stopId == p.PlatformTag).Times
                    ))
                    .Where(ds => ds.Times != null)
                    .ToList()
                ))
                .Where(r => r.DaySchedules.Any())
            );

            return result;
        }
    }
}
