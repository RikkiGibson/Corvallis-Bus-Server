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

            var currentTime = getCurrentTime();

            var todaySchedule = stopIds.Where(schedule.ContainsKey).ToDictionary(platformNo => platformNo,
                platformNo => schedule[platformNo].ToDictionary(routeSchedule => routeSchedule.RouteNo,
                    routeSchedule =>
                    {
                        var result = new List<int>();
                        
                        var scheduleCutoff = currentTime.TimeOfDay.Add(TimeSpan.FromMinutes(30));
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
                                                 .Select(ts => (int)ts.Subtract(currentTime.TimeOfDay).TotalMinutes));
                        }
                        return result;
                    }));

            return todaySchedule;
        }

        /// <summary>
        /// Shamelessly c/ped from stackoverflow.
        /// </summary>
        public static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }

        private static string ToArrivalsSummary(List<int> arrivalTimes, Func<DateTimeOffset> getCurrentTime)
        {
            if (arrivalTimes.Count == 0)
            {
                return "No arrivals!";
            }

            var currentTime = getCurrentTime();
            var summaries = arrivalTimes.Take(2).Select(t => t > 30 ? currentTime.AddMinutes(t).ToString("h:mm tt") : $"{t} minutes");
            return string.Join(", ", summaries);
        }

        public static async Task<List<FavoriteStopViewModel>> GetFavoritesViewModel(ITransitRepository repository,
            Func<DateTimeOffset> getCurrentTime, string[] stopIds, LatLong? optionalUserLocation, bool fallbackToGrayColor)
        {
            var staticData = JsonConvert.DeserializeObject<BusStaticData>(await repository.GetStaticDataAsync());

            // TODO: controllers should handle integer parsing *once* and
            // spurious conversions between strings and integers shouldn't be necessary.
            int parseResult;
            var intIds = stopIds.Select(id => int.TryParse(id, out parseResult) ? parseResult : 0);

            var favoriteStops = intIds.Where(staticData.Stops.ContainsKey).Select(id =>
            {
                var stop = staticData.Stops[id];
                return new
                {
                    stopId = stop.ID,
                    stopName = stop.Name,
                    routes = stop.RouteNames,
                    distanceFromUser = optionalUserLocation != null
                        ? DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, stop.Lat, stop.Long, 'M')
                        : double.NaN,
                    isNearestStop = false
                };
            })
            .ToList();

            var nearestStop = optionalUserLocation != null
                ? staticData.Stops.Values
                    .Aggregate((s1, s2) =>
                        DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, s1.Lat, s1.Long, 'M') <
                        DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, s2.Lat, s2.Long, 'M') ? s1 : s2)
                : null;

            if (nearestStop != null && !favoriteStops.Any(f => f.stopId == nearestStop.ID))
            {
                favoriteStops.Add(new
                {
                    stopId = nearestStop.ID,
                    stopName = nearestStop.Name,
                    routes = nearestStop.RouteNames,
                    distanceFromUser = optionalUserLocation != null
                        ? DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, nearestStop.Lat, nearestStop.Long, 'M')
                        : double.NaN,
                    isNearestStop = true
                });
            }

            favoriteStops.Sort((f1, f2) => f1.distanceFromUser.CompareTo(f2.distanceFromUser));
            
            var etaStops = favoriteStops.Select(f => f.stopId.ToString()).ToArray();
            var schedule = await GetSchedule(repository, getCurrentTime, etaStops);

            var fallbackColor = fallbackToGrayColor ? "AAAAAA" : string.Empty;

            var result = favoriteStops.Select(a =>
            {
                var routeSchedules = schedule[a.stopId.ToString()]
                    .Where(rs => rs.Value.Any())
                    .OrderBy(rs => rs.Value.Aggregate(int.MaxValue, Math.Min))
                    .Take(2)
                    .ToList();

                var firstRoute = routeSchedules.Count > 0 ? staticData.Routes[routeSchedules[0].Key] : null;
                var secondRoute = routeSchedules.Count > 1 ? staticData.Routes[routeSchedules[1].Key] : null;

                return new FavoriteStopViewModel
                {
                    StopId = a.stopId,
                    StopName = a.stopName,

                    FirstRouteName = firstRoute != null ? firstRoute.RouteNo : string.Empty,
                    FirstRouteColor = firstRoute != null ? firstRoute.Color : fallbackColor,
                    FirstRouteArrivals = routeSchedules.Count > 0 ? ToArrivalsSummary(routeSchedules[0].Value, getCurrentTime) : string.Empty,

                    SecondRouteName = secondRoute != null ? secondRoute.RouteNo : string.Empty,
                    SecondRouteColor = secondRoute != null ? secondRoute.Color : string.Empty,
                    SecondRouteArrivals = routeSchedules.Count > 1 ? ToArrivalsSummary(routeSchedules[1].Value, getCurrentTime) : string.Empty,

                    DistanceFromUser = double.IsNaN(a.distanceFromUser) ? "" : $"{a.distanceFromUser:F1} miles",
                    IsNearestStop = a.isNearestStop
                };
            }).ToList();

            return result;
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
