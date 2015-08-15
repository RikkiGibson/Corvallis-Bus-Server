using API.DataAccess;
using API.Models;
using API.WebClients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    public static class TransitManager
    {
        /// <summary>
        /// Performs route and stop lookups and builds the static data used in the /static route.
        /// </summary>
        public static async Task<object> GetStaticData(ITransitRepository repository)
        {
            // TODO: implement static data dump in repository so we don't needlessly deserialize and reserialize
            var routesJson = await repository.GetRoutesAsync();
            var stopsJson = await repository.GetStopsAsync();

            var routes = JsonConvert.DeserializeObject<List<BusRoute>>(routesJson);
            var stops = JsonConvert.DeserializeObject<List<BusStop>>(stopsJson);

            return new
            {
                routes = routes.ToDictionary(r => r.RouteNo),
                stops = stops.ToDictionary(s => s.ID)
            };
        }
        
        /// <summary>
        /// Returns the bus schedule for the given stop IDs, incorporating the ETA from Connexionz.
        /// </summary>
        public static async Task<Dictionary<string, Dictionary<string, IEnumerable<string>>>> GetSchedule(ITransitRepository repository, IEnumerable<string> stopIds)
        {
            // This is a good candidate for TDD -- maybe time to actually create the IBusRepository.
            var scheduleJson = await repository.GetScheduleAsync();
            var schedule = JsonConvert.DeserializeObject<Dictionary<string, IEnumerable<BusStopRouteSchedule>>>(scheduleJson);

            var estimates = await TransitClient.GetEtas(repository, stopIds);

            var todaySchedule = stopIds.Where(schedule.ContainsKey).ToDictionary(platformNo => platformNo,
                platformNo => schedule[platformNo].ToDictionary(routeSchedule => routeSchedule.RouteNo,
                    routeSchedule =>
                    {
                        var daySchedule = routeSchedule.DaySchedules.First(ds => DaysOfWeekUtils.IsToday(ds.Days));

                        var now = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, "Pacific Standard Time");
                        var midnight = now.Subtract(now.TimeOfDay);

                        var scheduleCutoff = now.AddMinutes(30);

                        var dateTimesList = new List<DateTimeOffset>();

                        // If an estimate is present, fold it in.
                        if (estimates.ContainsKey(platformNo))
                        {
                            var stopEstimate = estimates[platformNo];
                            if (stopEstimate.ContainsKey(routeSchedule.RouteNo))
                            {
                                var routeEstimate = stopEstimate[routeSchedule.RouteNo];
                                var estimate = now.AddMinutes(routeEstimate);
                                dateTimesList.Add(estimate);
                            }
                        }

                        dateTimesList.AddRange(
                            daySchedule.Times.Select(t => midnight.Add(t))
                                             .Where(dt => dt > scheduleCutoff));
                        return dateTimesList.Select(dt => dt.ToString("yyyy-MM-dd HH:mm zzz"));
                    }));

            return todaySchedule;
        }

        public static Dictionary<string, Dictionary<string, int>> GetEtas(ITransitRepository repository, IEnumerable<string> etas)
        {
            throw new NotImplementedException("TODO");
        }
    }
}
