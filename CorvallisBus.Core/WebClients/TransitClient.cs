using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using CorvallisBus.Core.DataAccess;
using Newtonsoft.Json;

namespace CorvallisBus.Core.WebClients
{
    using ServerBusSchedule = Dictionary<int, IEnumerable<BusStopRouteSchedule>>;


    /// <summary>
    /// Merges data obtained from Connexionz and Google Transit
    /// and makes it ready for delivery to clients.
    /// </summary>
    public class TransitClient : ITransitClient
    {
        public BusSystemData LoadTransitData()
        {
            var connexionzPlatforms = ConnexionzClient.LoadPlatforms();
            var connexionzRoutes = ConnexionzClient.LoadRoutes();
            var googleData = GoogleTransitClient.LoadData();

            var routes = CreateRoutes(googleData.Routes, connexionzRoutes);
            var stops = CreateStops(connexionzPlatforms, connexionzRoutes);
            
            var staticData = new BusStaticData
            {
                Routes = routes.ToDictionary(r => r.RouteNo),
                Stops = stops.ToDictionary(s => s.ID)
            };

            var platformTagsLookup = connexionzPlatforms.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);
            var schedule = CreateSchedule(googleData.Schedules, connexionzRoutes, connexionzPlatforms);

            return new BusSystemData(
                staticData,
                schedule,
                platformTagsLookup);
        }

        private static bool ShouldAppendDirection(ConnexionzPlatform platform, List<ConnexionzPlatform> platforms)
        {
            if (platform.Name == "Downtown Transit Center")
                return false;

            bool existsSameNamedStop = platforms.Any(p => p.PlatformNo != platform.PlatformNo && p.Name == platform.Name);
            return existsSameNamedStop;
        }

        private static List<BusStop> CreateStops(List<ConnexionzPlatform> platforms, List<ConnexionzRoute> routes)
        {
            return platforms
                .Select(p => 
                    new BusStop(p,
                        routes.Where(r => r.Path.Any(rp => rp.PlatformId == p.PlatformNo))
                            .Select(r => r.RouteNo)
                            .ToList(),
                        ShouldAppendDirection(p, platforms)))
                .Where(r => r.RouteNames.Any())
                .ToList();
        }

        private static List<BusRoute> CreateRoutes(List<GoogleRoute> googleRoutes, List<ConnexionzRoute> connexionzRoutes)
        {
            var googleRoutesDict = googleRoutes.ToDictionary(gr => gr.ConnexionzName);
            var routes = connexionzRoutes.Where(r => googleRoutesDict.ContainsKey(r.RouteNo));
            return routes.Select(r => new BusRoute(r, googleRoutesDict)).ToList();
        }

        public async Task<ConnexionzPlatformET> GetEta(int platformTag) => await ConnexionzClient.GetPlatformEta(platformTag);

        private static TimeSpan RoundToNearestMinute(TimeSpan source)
        {
            // there are 10,000 ticks per millisecond, and 1000 milliseconds per second, and 60 seconds per minute
            const int TICKS_PER_MINUTE = 10000 * 1000 * 60;
            var subMinutesComponent = source.Ticks % TICKS_PER_MINUTE;

            return subMinutesComponent < TICKS_PER_MINUTE / 2
                ? source.Subtract(TimeSpan.FromTicks(subMinutesComponent))
                : source.Add(TimeSpan.FromTicks(TICKS_PER_MINUTE - subMinutesComponent));
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
                        schedule[i].Times.Zip(stepSizes, (time, step) => RoundToNearestMinute(time.Add(TimeSpan.FromTicks(step * idx))))
                                         .ToList())));
            }

            return results;
        }
        
        /// <summary>
        /// Creates a bus schedule based on Google Transit data.
        /// </summary>
        public ServerBusSchedule CreateSchedule(
            List<GoogleRouteSchedule> googleSchedules,
            List<ConnexionzRoute> connexionzRoutes,
            List<ConnexionzPlatform> connexionzPlatforms)
        {
            var googleSchedulesDict = googleSchedules.ToDictionary(schedule => schedule.ConnexionzName);
            var routes = connexionzRoutes.Where(r => r.IsActive && googleSchedulesDict.ContainsKey(r.RouteNo));

            // Build all the schedule data for intermediate stops.
            var routeSchedules = routes.Select(r => new
            {
                routeNo = r.RouteNo,
                daySchedules = googleSchedulesDict[r.RouteNo].Days.Select(
                    d => new
                    {
                        days = d.Days,
                        stopSchedules = InterpolateSchedule(r, d.StopSchedules)
                    })
            });

            // Now turn it on its head so it's easy to query from a stop-oriented way.
            var result = connexionzPlatforms.ToDictionary(p => p.PlatformNo,
                p => routeSchedules.Select(r => new BusStopRouteSchedule
                {
                    RouteNo = r.routeNo,
                    DaySchedules = r.daySchedules.Select(ds => new BusStopRouteDaySchedule
                    {
                        Days = ds.days,
                        Times = ds.stopSchedules.FirstOrDefault(ss => ss.Item1 == p.PlatformNo)?.Item2
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