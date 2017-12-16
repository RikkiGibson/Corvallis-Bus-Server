using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.WebClients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus
{
    // Maps a stop ID to a dictionary that maps a route number to a list of arrival times.
    // Intended for client consumption.
    using ClientBusSchedule = Dictionary<int, Dictionary<string, List<BusArrivalTime>>>;

    // Maps a 5-digit stop ID to a dictionary that maps a route number to an arrival estimate in minutes.
    // Exists to provide some compile-time semantics to differ between schedules and estimates.
    using BusArrivalEstimates = Dictionary<int, Dictionary<string, List<int>>>;

    public static class TransitManager
    {
        /// <summary>
        /// The greatest number of minutes from now that an estimate can have.
        /// </summary>
        public const int ESTIMATES_MAX_ADVANCE_MINUTES = 30;

        /// <summary>
        /// The range of minutes in which an estimate time can replace a scheduled time.
        /// </summary>
        public const int ESTIMATE_CORRELATION_TOLERANCE_MINUTES = 10;

        /// <summary>
        /// The smallest number of minutes from now that a scheduled time can be rendered.
        /// </summary>
        public const int SCHEDULE_CUTOFF_MINUTES = 20;

        /// <summary>
        /// Returns the bus schedule for the given stop IDs, incorporating the ETA from Connexionz.
        /// </summary>
        public static async Task<ClientBusSchedule> GetSchedule(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime, IEnumerable<int> stopIds)
        {
            var schedulesTask = repository.GetScheduleAsync();
            var estimatesTask = GetEtas(repository, client, stopIds);

            var schedule = await schedulesTask;
            var estimates = await estimatesTask;
            
            Func<int, Dictionary<string, List<BusArrivalTime>>> makePlatformSchedule = platformNo =>
                schedule[platformNo].ToDictionary(routeSchedule => routeSchedule.RouteNo,
                    routeSchedule => InterleaveRouteScheduleAndEstimates(
                        routeSchedule,
                        estimates.ContainsKey(platformNo)
                            ? estimates[platformNo]
                            : new Dictionary<string, List<int>>(),
                        currentTime));

            var todaySchedule = stopIds.Where(schedule.ContainsKey)
                                       .ToDictionary(platformNo => platformNo, makePlatformSchedule);

            return todaySchedule;
        }

        private static List<BusArrivalTime> InterleaveRouteScheduleAndEstimates(BusStopRouteSchedule routeSchedule,
            Dictionary<string, List<int>> stopEstimates, DateTimeOffset currentTime)
        {
            var arrivalTimes = Enumerable.Empty<BusArrivalTime>();

            var daySchedule = routeSchedule.DaySchedules.FirstOrDefault(ds => DaysOfWeekUtils.IsToday(ds.Days, currentTime));
            if (daySchedule != null)
            {
                var relativeSchedule = MakeRelativeScheduleWithinCutoff(daySchedule, currentTime);
                arrivalTimes = relativeSchedule.Select(minutes => new BusArrivalTime(minutes, isEstimate: false));
            }
            
            if (stopEstimates.ContainsKey(routeSchedule.RouteNo))
            {
                var routeEstimates = stopEstimates[routeSchedule.RouteNo];
                arrivalTimes = arrivalTimes.Where(arrivalTime =>
                        !routeEstimates.Any(estimate =>
                            Math.Abs(arrivalTime.MinutesFromNow - estimate) <= ESTIMATE_CORRELATION_TOLERANCE_MINUTES));
                arrivalTimes = arrivalTimes.Concat(
                    routeEstimates.Select(estimate => new BusArrivalTime(estimate, isEstimate: true)));
            }

            arrivalTimes = arrivalTimes.OrderBy(arrivalTime => arrivalTime);

            return arrivalTimes.ToList();
        }

        private static IEnumerable<int> MakeRelativeScheduleWithinCutoff(BusStopRouteDaySchedule daySchedule, DateTimeOffset currentTime)
        {
            // Is there a better condition for this, i.e. involving a check whether there are 24hr+ time spans in the schedule?
            var timeOfDay = daySchedule.Days == DaysOfWeek.NightOwl &&
                currentTime.TimeOfDay.Hours < 4
                ? currentTime.TimeOfDay.Add(TimeSpan.FromDays(1))
                : currentTime.TimeOfDay;

            var scheduleCutoff = timeOfDay.Add(TimeSpan.FromMinutes(20));

            return daySchedule.Times.Where(ts => ts > scheduleCutoff)
                .Select(ts => (int)ts.Subtract(timeOfDay).TotalMinutes);
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

        private static List<FavoriteStop> GetFavoriteStops(BusStaticData staticData, IEnumerable<int> stopIds, LatLong? optionalUserLocation)
        {
            var favoriteStops = stopIds.Where(staticData.Stops.ContainsKey).Select(id =>
            {
                var stop = staticData.Stops[id];
                var distanceFromUser = optionalUserLocation != null
                        ? DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, stop.Lat, stop.Long, 'M')
                        : double.NaN;

                return new FavoriteStop(stop.ID, stop.Name, stop.RouteNames, distanceFromUser, isNearestStop: false);
            })
            .ToList();

            var nearestStop = optionalUserLocation != null
                ? staticData.Stops.Values
                    .Aggregate((s1, s2) =>
                        DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, s1.Lat, s1.Long, 'M') <
                        DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, s2.Lat, s2.Long, 'M') ? s1 : s2)
                : null;

            if (nearestStop != null && !favoriteStops.Any(f => f.Id == nearestStop.ID))
            {
                var distanceFromUser = optionalUserLocation != null
                    ? DistanceTo(optionalUserLocation.Value.Lat, optionalUserLocation.Value.Lon, nearestStop.Lat, nearestStop.Long, 'M')
                    : double.NaN;

                favoriteStops.Add(new FavoriteStop(nearestStop.ID, nearestStop.Name, nearestStop.RouteNames,
                                                   distanceFromUser, isNearestStop: true));
            }

            favoriteStops.Sort((f1, f2) => f1.DistanceFromUser.CompareTo(f2.DistanceFromUser));

            return favoriteStops;
        }

        private static readonly BusArrivalTime ARRIVAL_TIME_SEED = new BusArrivalTime(int.MaxValue, isEstimate: false);
        private static FavoriteStopViewModel ToViewModel(FavoriteStop favorite, BusStaticData staticData,
            ClientBusSchedule schedule, DateTimeOffset currentTime)
        {
            var routeSchedules = schedule[favorite.Id]
                       .Where(rs => rs.Value.Any())
                       .OrderBy(rs => rs.Value.Aggregate(ARRIVAL_TIME_SEED, BusArrivalTime.Min))
                       .Take(2)
                       .ToList();

            var firstRoute = routeSchedules.Count > 0 ? staticData.Routes[routeSchedules[0].Key] : null;
            var secondRoute = routeSchedules.Count > 1 ? staticData.Routes[routeSchedules[1].Key] : null;

            return new FavoriteStopViewModel
            {
                StopId = favorite.Id,
                StopName = favorite.Name,

                FirstRouteName = firstRoute != null ? firstRoute.RouteNo : string.Empty,
                FirstRouteColor = firstRoute != null ? firstRoute.Color : string.Empty,
                FirstRouteArrivals = routeSchedules.Count > 0 ? RouteArrivalsSummary.ToEstimateSummary(routeSchedules[0].Value, currentTime) : "No arrivals!",

                SecondRouteName = secondRoute != null ? secondRoute.RouteNo : string.Empty,
                SecondRouteColor = secondRoute != null ? secondRoute.Color : string.Empty,
                SecondRouteArrivals = routeSchedules.Count > 1 ? RouteArrivalsSummary.ToEstimateSummary(routeSchedules[1].Value, currentTime) : string.Empty,

                DistanceFromUser = double.IsNaN(favorite.DistanceFromUser) ? "" : $"{favorite.DistanceFromUser:F1} miles",
                IsNearestStop = favorite.IsNearestStop
            };
        }

        public static async Task<List<FavoriteStopViewModel>> GetFavoritesViewModel(ITransitRepository repository,
            ITransitClient client, DateTimeOffset currentTime, IEnumerable<int> stopIds, LatLong? optionalUserLocation)
        {
            var staticData = await repository.GetStaticDataAsync();

            var favoriteStops = GetFavoriteStops(staticData, stopIds, optionalUserLocation);

            var scheduleTask = GetSchedule(repository, client, currentTime, favoriteStops.Select(f => f.Id));

            var schedule = await scheduleTask;

            var result = favoriteStops.Select(favorite => ToViewModel(favorite, staticData, schedule, currentTime))
                                      .ToList();

            return result;
        }

        /// <summary>
        /// Gets the ETA info for a set of stop IDS.
        /// The outer dictionary takes a route number and gives a dictionary that takes a stop ID to an ETA.
        /// </summary>
        public static async Task<BusArrivalEstimates> GetEtas(ITransitRepository repository, ITransitClient client, IEnumerable<int> stopIds)
        {
            var toPlatformTag = await repository.GetPlatformTagsAsync();
            
            Func<int, Task<Tuple<int, ConnexionzPlatformET>>> getEtaIfTagExists =
                async id => Tuple.Create(id, toPlatformTag.ContainsKey(id) ? await client.GetEta(toPlatformTag[id]) : null);

            var tasks = stopIds.Select(getEtaIfTagExists);
            var results = await Task.WhenAll(tasks);

            return results.ToDictionary(eta => eta.Item1,
                eta => eta.Item2?.RouteEstimatedArrivals
                                ?.ToDictionary(routeEta => routeEta.RouteNo,
                                               routeEta => routeEta.EstimatedArrivalTime)
                       ?? new Dictionary<string, List<int>>());
        }

        /// <summary>
        /// Gets a user friendly arrivals summary for the requested stops.
        /// Returns a dictionary which takes a stop ID and returns the list of route arrival summaries (used to populate a table).
        /// </summary>
        public static async Task<Dictionary<int, List<RouteArrivalsSummary>>> GetArrivalsSummary(ITransitRepository repository, ITransitClient client, DateTimeOffset currentTime, IEnumerable<int> stopIds)
        {
            var schedule = await GetSchedule(repository, client, currentTime, stopIds);
            var staticData = await repository.GetStaticDataAsync();

            var matchingStopIds = stopIds.Where(staticData.Stops.ContainsKey);
            var arrivalsSummaries = matchingStopIds.ToDictionary(stopId => stopId,
                stopId => ToRouteArrivalsSummaries(staticData.Stops[stopId].RouteNames, schedule[stopId], currentTime));
            return arrivalsSummaries;
        }

        private static List<RouteArrivalsSummary> ToRouteArrivalsSummaries(List<string> routeNames,
            Dictionary<string, List<BusArrivalTime>> stopArrivals, DateTimeOffset currentTime)
        {
            var arrivalsSummaries =
                routeNames.Select(routeName =>
                            new KeyValuePair<string, List<BusArrivalTime>>(routeName,
                                stopArrivals.ContainsKey(routeName) ? stopArrivals[routeName] : new List<BusArrivalTime>()))
                          .OrderBy(kvp => kvp.Value.DefaultIfEmpty(ARRIVAL_TIME_SEED).Min())
                          .Select(kvp => new RouteArrivalsSummary(routeName: kvp.Key,
                                routeArrivalTimes: kvp.Value, currentTime: currentTime))
                          .ToList();

            return arrivalsSummaries;
        }
    }
}
