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
    // Maps a stop ID to a dictionary that maps a route number to a list of arrival times.
    // Intended for client consumption.
    using ClientBusSchedule = Dictionary<string, Dictionary<string, IEnumerable<string>>>;

    public static class TransitManager
    {
        /// <summary>
        /// Returns the bus schedule for the given stop IDs, incorporating the ETA from Connexionz.
        /// </summary>
        public static async Task<ClientBusSchedule> GetSchedule(ITransitRepository repository, IEnumerable<string> stopIds)
        {
            var schedule = await repository.GetScheduleAsync();
            var toPlatformTag = await repository.GetPlatformTagsAsync();
            var estimates = await TransitClient.GetEtas(toPlatformTag, stopIds);

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
