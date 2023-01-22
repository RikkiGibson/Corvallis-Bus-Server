using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using CorvallisBus.Core.DataAccess;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Net;

namespace CorvallisBus.Core.WebClients
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;

    /// <summary>
    /// Merges data obtained from Connexionz and Google Transit
    /// and makes it ready for delivery to clients.
    /// </summary>
    public class TransitClient : ITransitClient
    {
        public (BusSystemData data, List<string> errors) LoadTransitData()
        {
            var connexionzPlatforms = ConnexionzClient.LoadPlatforms();
            var connexionzRoutes = ConnexionzClient.LoadRoutes();
            var googleData = GoogleTransitClient.LoadData();

            var routes = CreateRoutes(googleData.Routes, connexionzRoutes);
            var stops = CreateStops(connexionzPlatforms, connexionzRoutes);

            var staticData = new BusStaticData(
                Routes: routes.ToDictionary(r => r.RouteNo),
                Stops: stops.ToDictionary(s => s.Id)
            );

            var platformTagsLookup = connexionzPlatforms.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);
            var schedule = CreateSchedule(googleData.Schedules, connexionzRoutes, connexionzPlatforms);

            var transitData = new BusSystemData(
                staticData,
                schedule,
                platformTagsLookup);

            var errors = ValidateTransitData(transitData);

            return (transitData, errors);
        }

        public static List<string> ValidateTransitData(BusSystemData data)
        {
            var errors = new List<string>();

            // Validate schedule within each stop
            foreach (var kvp in data.Schedule)
            {
                var (stopId, stopRouteSchedules) = (kvp.Key, kvp.Value);
                foreach (var routeSchedule in stopRouteSchedules)
                {
                    DaysOfWeek usedDays = 0;
                    foreach (var routeDaySchedule in routeSchedule.DaySchedules)
                    {
                        if (routeDaySchedule.Days == DaysOfWeek.None)
                        {
                            errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has a day schedule for DaysOfWeek.None.");
                        }

                        if ((routeDaySchedule.Days & usedDays) != 0)
                        {
                            errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has a overlapping day schedule {usedDays & routeDaySchedule.Days}");
                        }

                        usedDays = usedDays | routeDaySchedule.Days;

                        var currentTime = TimeSpan.MinValue;
                        foreach (var nextTime in routeDaySchedule.Times)
                        {
                            if (nextTime <= currentTime)
                            {
                                errors.Add($"Route {routeSchedule.RouteNo} at stop {stopId} has an ordering discrepancy in its schedule. Arrival time {currentTime} is followed by {nextTime}");
                            }
                            currentTime = nextTime;
                        }
                    }
                }
            }

            // Validate schedule for each route
            foreach (var route in data.StaticData.Routes.Values)
            {
                var firstStopId = route.Path[0];
                var firstStopSchedules = data.Schedule[firstStopId].FirstOrDefault(rs => rs.RouteNo == route.RouteNo);
                if (firstStopSchedules == null)
                {
                    errors.Add($"Route {route.RouteNo} has no schedule for stop ID {firstStopId}");
                    continue;
                }

                foreach (var firstStopDaySchedule in firstStopSchedules.DaySchedules)
                {
                    for (var arrivalNo = 0; arrivalNo < firstStopDaySchedule.Times.Count; arrivalNo++)
                    {
                        // get the i'th arrival for each stop in the path, ensure monotonically increasing
                        var currentArrivalTime = TimeSpan.MinValue;

                        for (var stopIdx = 0; stopIdx < route.Path.Count; stopIdx++)
                        {
                            var stopId = route.Path[stopIdx];
                            if (stopId == 0)
                            {
                                errors.Add($"Route {route.RouteNo} has a missing stop in its path at index {stopIdx}");
                                continue;
                            }
                            else if (stopId < 1000)
                            {
                                errors.Add($"Route {route.RouteNo} is using platform tag {stopId} as an ID because it has no stop ID");
                                continue;
                            }
                            else if (!data.Schedule.ContainsKey(stopId))
                            {
                                errors.Add($"Route {route.RouteNo} is using stop ID {stopId} which has no schedule");
                                continue;
                            }

                            var routeStopDayArrivalTimes = data
                                .Schedule[stopId]
                                .Single(rs => rs.RouteNo == route.RouteNo)
                                .DaySchedules
                                .Single(ds => ds.Days == firstStopDaySchedule.Days)
                                .Times;

                            if (routeStopDayArrivalTimes.Count != firstStopDaySchedule.Times.Count)
                            {
                                errors.Add($"Warning: {route.RouteNo} does not have the same number of arrivals at all stops. Stop {stopId} has {routeStopDayArrivalTimes.Count} arrivals while stop ID {firstStopId} has {routeStopDayArrivalTimes.Count} arrivals.");
                                continue;
                            }

                            var nextArrivalTime = routeStopDayArrivalTimes[arrivalNo];
                            if (nextArrivalTime <= currentArrivalTime)
                            {
                                Debug.Assert(stopIdx > 0);
                                errors.Add($"Route {route.RouteNo} has a schedule discrepancy across stops {route.Path[stopIdx - 1]} and {route.Path[stopIdx]}. Arrival time {currentArrivalTime} is followed by {nextArrivalTime}");
                            }

                            currentArrivalTime = nextArrivalTime;
                        }
                    }
                }
            }

            return errors.Distinct().ToList();
        }

        private static bool ShouldAppendDirection(ConnexionzPlatform platform, List<ConnexionzPlatform> platforms)
        {
            if (platform.Name == "Downtown Transit Center" || platform.Name == "Downtown Transit Centre")
                return false;

            bool existsSameNamedStop = platforms.Any(p => p.PlatformNo != platform.PlatformNo && p.CompactName == platform.CompactName);
            return existsSameNamedStop;
        }

        private static List<BusStop> CreateStops(List<ConnexionzPlatform> platforms, List<ConnexionzRoute> routes)
        {
            return platforms
                .Select(p =>
                    BusStop.Create(p,
                        routes.Where(r => r.Path.Any(rp => rp.PlatformId == p.PlatformNo))
                            .Select(r => r.RouteNo)
                            .ToList(),
                        ShouldAppendDirection(p, platforms)))
                .Where(r => r.RouteNames.Any())
                .ToList();
        }

        private static List<BusRoute> CreateRoutes(List<GoogleRoute> googleRoutes, List<ConnexionzRoute> connexionzRoutes)
        {
            var googleRoutesDict = googleRoutes.ToDictionary(gr => gr.Name);
            var routes = connexionzRoutes.Where(r => r.IsActive && googleRoutesDict.ContainsKey(r.RouteNo));
            return routes.Select(r => BusRoute.Create(r, googleRoutesDict)).ToList();
        }

        public async Task<ConnexionzPlatformET?> GetEta(int platformTag) => await ConnexionzClient.GetPlatformEta(platformTag);

        /// <summary>
        /// Creates a bus schedule based on Google Transit data.
        /// </summary>
        public ServerBusSchedule CreateSchedule(
            List<GoogleRouteSchedule> googleSchedules,
            List<ConnexionzRoute> connexionzRoutes,
            List<ConnexionzPlatform> connexionzPlatforms)
        {
            var googleSchedulesDict = googleSchedules.ToDictionary(schedule => schedule.RouteNo);
            var routes = connexionzRoutes.Where(r => r.IsActive && googleSchedulesDict.ContainsKey(r.RouteNo));

            var routeSchedules = routes.Select(r => new
            {
                routeNo = r.RouteNo,
                daySchedules = googleSchedulesDict[r.RouteNo].Days.Select(
                    d => new
                    {
                        days = d.Days,
                        stopSchedules = d.StopSchedules.Zip(r.Path, (ss, stop) => (stop.PlatformId, ss.Times))
                    })
            });

            // Now turn it on its head so it's easy to query from a stop-oriented way.
            var result = connexionzPlatforms.ToDictionary(p => p.PlatformNo,
                // TODO: change type to List?
                p => routeSchedules.Select(r => new BusStopRouteSchedule(
                    RouteNo: r.routeNo,
                    DaySchedules: r.daySchedules.Select(ds => new BusStopRouteDaySchedule(
                        Days: ds.days,
                        Times: ds.stopSchedules.FirstOrDefault(ss => ss.PlatformId == p.PlatformNo || ss.PlatformId == p.PlatformTag).Times
                    ))
                    .Where(ds => ds.Times != null)
                    .ToList()
                ))
                .Where(r => r.DaySchedules.Any())
            );

            return result;
        }

        public Task<List<ServiceAlert>> GetServiceAlerts() => ServiceAlertsClient.GetServiceAlerts();
    }
}