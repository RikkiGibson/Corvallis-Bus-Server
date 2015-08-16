using API.DataAccess;
using API.Models;
using API.Models.Connexionz;
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
    using ClientBusSchedule = Dictionary<string, Dictionary<string, List<int>>>;
    
    // Maps a 5-digit stop ID to a dictionary that maps a route number to an arrival estimate in minutes.
    // TODO: these are awfully similar...should this stil exist?
    using BusArrivalEstimates = Dictionary<string, Dictionary<string, List<int>>>;

    public static class TransitManager
    {
        /// <summary>
        /// Returns the bus schedule for the given stop IDs, incorporating the ETA from Connexionz.
        /// </summary>
        public static async Task<ClientBusSchedule> GetSchedule(ITransitRepository repository, Func<DateTimeOffset> getCurrentTime, string[] stopIds)
        {
            var schedule = await repository.GetScheduleAsync();
            var toPlatformTag = await repository.GetPlatformTagsAsync();
            var estimates = await GetEtas(repository, stopIds);

            var todaySchedule = stopIds.Where(schedule.ContainsKey).ToDictionary(platformNo => platformNo,
                platformNo => schedule[platformNo].ToDictionary(routeSchedule => routeSchedule.RouteNo,
                    routeSchedule =>
                    {
                        var result = new List<int>();
                        
                        var scheduleCutoff = getCurrentTime().TimeOfDay.Add(TimeSpan.FromMinutes(30));
                        if (estimates.ContainsKey(platformNo))
                        {
                            var stopEstimate = estimates[platformNo];
                            if (stopEstimate.ContainsKey(routeSchedule.RouteNo))
                            {
                                var routeEstimates = stopEstimate[routeSchedule.RouteNo];
                                result.AddRange(routeEstimates);
                            }
                        }

                        var daySchedule = routeSchedule.DaySchedules.FirstOrDefault(ds => DaysOfWeekUtils.IsToday(ds.Days, getCurrentTime));
                        if (daySchedule != null)
                        {
                            result.AddRange(
                                daySchedule.Times.Where(ts => ts > scheduleCutoff)
                                                 .Select(ts => (int)ts.TotalMinutes));
                        }
                        return result;
                    }));

            return todaySchedule;
        }

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.  Performs the calls to get the info in parallel,
        /// aggregating the data into a dictionary.
        /// The outer dictionary takes a route number and gives a dictionary that takes a stop ID to an ETA.
        /// </summary>
        public static async Task<BusArrivalEstimates> GetEtas(ITransitRepository repository, string[] stopIds)
        {
            var toPlatformTag = await repository.GetPlatformTagsAsync();
            
            // TODO: fetch ETAs from cache. How will that data be structured?
            Func<string, Task<Tuple<string, ConnexionzPlatformET>>> getEtaIfTagExists =
                async id => Tuple.Create(id, toPlatformTag.ContainsKey(id) ? await TransitClient.GetEta(toPlatformTag[id]) : null);

            var tasks = stopIds.Select(getEtaIfTagExists);
            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(eta => eta.Item1,
                eta => eta.Item2?.RouteEstimatedArrivals
                                ?.ToDictionary(routeEta => routeEta.RouteNo,
                                               routeEta => routeEta.EstimatedArrivalTime)
                       ?? new Dictionary<string, List<int>>());
        }
    }
}
