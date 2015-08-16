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
    using System.Device.Location;

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

        public static IEnumerable<T> EnumerateSingle<T>(T item)
        {
            yield return item;
        }

        public static async Task<List<FavoriteStopViewModel>> GetFavoritesViewModel(ITransitRepository repository, Func<DateTimeOffset> getCurrentTime,
            string[] stopIds, GeoCoordinate optionalUserLocation, bool fallbackToGrayColor)
        {
            var staticData = JsonConvert.DeserializeObject<BusStaticData>(await repository.GetStaticDataAsync());

            // TODO: don't create, throw away and recreate GeoCoordinates
            //var favoriteDistances = stopIds.Select(id => new
            //{

            //    {

            //    })
            //}

            var nearestStop = optionalUserLocation != null
                ? staticData.Stops.Values
                    .Aggregate((s1, s2) =>
                        optionalUserLocation.GetDistanceTo(new GeoCoordinate(s1.Lat, s1.Long)) <
                        optionalUserLocation.GetDistanceTo(new GeoCoordinate(s2.Lat, s2.Long)) ? s1 : s2)
                : null;

            // If present, include the nearest stop when doing a schedule lookup.
            var etaStops = nearestStop != null ? stopIds.Concat(EnumerateSingle(nearestStop.ID.ToString())) : stopIds;
            var schedule = await GetSchedule(repository, getCurrentTime, etaStops.Distinct().ToArray());

            var favoriteStops = stopIds.Select(id =>
            {
                var stop = staticData.Stops[int.Parse(id)];
                return new
                {
                    stopId = stop.ID,
                    stopName = stop.Name,
                    routes = stop.RouteNames.Select(rn => new
                    {
                        route = staticData.Routes[rn],
                        arrivalTimes = schedule[id][rn]
                    }).OrderBy(a => a.arrivalTimes.Aggregate(int.MaxValue, Math.Min)).ToList(),
                    distanceFromUser = optionalUserLocation != null ? optionalUserLocation.GetDistanceTo(new GeoCoordinate(stop.Lat, stop.Long)) : double.NaN,
                    isFavoriteStop = false
                };
            })
            .OrderBy(b => b.distanceFromUser)
            .ToList();
            
            if (nearestStop != null && !favoriteStops.Any(f => f.stopId == nearestStop.ID))
            {
                favoriteStops.Insert(0, new
                {
                    stopId = nearestStop.ID,
                    stopName = nearestStop.Name,
                    routes = nearestStop.RouteNames.Select(rn => new
                    {
                        route = staticData.Routes[rn],
                        arrivalTimes = schedule[nearestStop.ID.ToString()][rn]
                    }).OrderBy(a => a.arrivalTimes.Aggregate(int.MaxValue, Math.Min)).ToList(),
                    distanceFromUser = optionalUserLocation != null ? optionalUserLocation.GetDistanceTo(new GeoCoordinate(nearestStop.Lat, nearestStop.Long)) : double.NaN,
                    isFavoriteStop = true
                });
            }

            var fallbackColor = fallbackToGrayColor ? "AAAAAA" : string.Empty;
            const double MILES_PER_METER = 0.000621371;

            var viewModel = favoriteStops.Select(a => new FavoriteStopViewModel
            {
                StopId = a.stopId,
                StopName = a.stopName,

                FirstRouteName = a.routes.Count > 0 && a.routes[0].arrivalTimes.Count > 0 ? a.routes[0].route.RouteNo : string.Empty,
                FirstRouteColor = a.routes.Count > 0 && a.routes[0].arrivalTimes.Count > 0 ? a.routes[0].route.Color : fallbackColor,
                FirstRouteArrivals = a.routes.Count > 0 ? ToArrivalsSummary(a.routes[0].arrivalTimes, getCurrentTime) : string.Empty,

                SecondRouteName = a.routes.Count > 1 && a.routes[1].arrivalTimes.Count > 0 ? a.routes[1].route.RouteNo : string.Empty,
                SecondRouteColor = a.routes.Count > 1 && a.routes[1].arrivalTimes.Count > 0 ? a.routes[1].route.Color : string.Empty,
                SecondRouteArrivals = a.routes.Count > 1 && a.routes[1].arrivalTimes.Count > 0 ? ToArrivalsSummary(a.routes[1].arrivalTimes, getCurrentTime) : string.Empty,

                DistanceFromUser = $"{a.distanceFromUser * MILES_PER_METER:F1} miles",
                IsNearestStop = false
            }).ToList();

            return viewModel;
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
