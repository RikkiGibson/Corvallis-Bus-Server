using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using CorvallisBus.Core.Properties;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CorvallisBus.Core.WebClients
{
    public class GoogleTransitData
    {
        public List<GoogleRoute> Routes { get; }
        public List<GoogleRouteSchedule> Schedules { get; }
        public List<GoogleStop> Stops { get; }

        public GoogleTransitData(
            List<GoogleRoute> routes,
            List<GoogleRouteSchedule> schedules,
            List<GoogleStop> stops)
        {
            Routes = routes;
            Schedules = schedules;
            Stops = stops;
        }
    }

    /// <summary>
    /// Contains the task for importing route and schedule data from Google Transit. This task is run once every night.
    /// </summary>
    public static class GoogleTransitClient
    {
        /// <summary>
        /// Downloads and interprets the ZIP file CTS uploads for Google. This is primarily to get route colors and route schedules.
        /// </summary>
        public static GoogleTransitData LoadData(ZipArchive archive)
        {
            var routesEntry = getEntry("routes.txt");

            var scheduleEntry = getEntry("stop_times.txt");
            using var scheduleCsv = new CsvReader(new StreamReader(scheduleEntry.Open()));
            var stopTimes = scheduleCsv.GetRecords<StopTimesEntry>().ToList();

            var tripsEntry = getEntry("trips.txt");
            using var tripsCsv = new CsvReader(new StreamReader(tripsEntry.Open()));
            var trips = tripsCsv.GetRecords<TripsEntry>().ToList();

            var calendarEntry = getEntry("calendar.txt");
            var stopsEntry = getEntry("stops.txt");
            var shapesEntry = getEntry("shapes.txt");

            var routes = ParseRouteCSV(routesEntry, trips, stopTimes, shapesEntry);
            var schedules = ParseScheduleCSV(stopTimes, trips, calendarEntry);
            var stops = ParseStopsCSV(stopsEntry);

            return new GoogleTransitData(
                routes: routes,
                schedules: schedules,
                stops: stops
            );

            ZipArchiveEntry getEntry(string filename) =>
                archive.GetEntry(filename) ?? throw new FileNotFoundException($"The Google Transit archive did not contain {filename}.");
        }

        private static List<GoogleStop> ParseStopsCSV(ZipArchiveEntry entry)
        {
            using var csv = new CsvReader(new StreamReader(entry.Open()));
            var records = csv.GetRecords<GoogleStop>();
            var stops = records.ToList();
            return stops;
        }

        private sealed class Comparer : IEqualityComparer<(string RouteId, IGrouping<int, ShapeEntry> shapeGroup)>
        {
            public bool Equals((string RouteId, IGrouping<int, ShapeEntry> shapeGroup) x, (string RouteId, IGrouping<int, ShapeEntry> shapeGroup) y)
            {
                return x.RouteId == y.RouteId;
            }

            public int GetHashCode((string RouteId, IGrouping<int, ShapeEntry> shapeGroup) obj)
            {
                return obj.GetHashCode();
            }

            private Comparer() { }
            public static Comparer Instance { get; } = new Comparer();
        }

        /// <summary>
        /// Reads a ZipArchive entry as the routes CSV and extracts the route colors and URLs.
        /// </summary>
        private static List<GoogleRoute> ParseRouteCSV(
            ZipArchiveEntry routesEntry,
            List<TripsEntry> trips,
            List<StopTimesEntry> stopTimes,
            ZipArchiveEntry shapesEntry)
        {
            using var routesCsv = new CsvReader(new StreamReader(routesEntry.Open()));
            var routesRecords = routesCsv.GetRecords<RouteEntry>();
            var routes = routesRecords.ToList();

            using var shapesCsv = new CsvReader(new StreamReader(shapesEntry.Open()));
            var shapesRecords = shapesCsv.GetRecords<ShapeEntry>();
            var shapes = shapesRecords.ToList();

            var shapesByRoute = (
                from shapeEntry in shapes
                group shapeEntry by shapeEntry.ShapeId into shapeGroup
                join trip in trips on shapeGroup.Key equals trip.ShapeId
                select (trip.RouteId, shapeGroup)
            ).Distinct(Comparer.Instance).ToList();

            var pathsByRoute = (
                from stopTime in stopTimes
                join trip in trips on stopTime.TripId equals trip.TripId
                orderby stopTime.StopSequence
                group stopTime.PlatformTag by trip.RouteId
            ).ToList();
            
            var fullRoutes =
                from route in routes
                // TODO: there are multiple paths for the same route.
                // Who even wants that.
                let path = pathsByRoute.First(p => p.Key == route.RouteNo)
                let shape = shapesByRoute.First(s => s.RouteId == route.RouteNo)
                let points = shape.shapeGroup.Select(point => new LatLong(point.ShapePointLat, point.ShapePointLon)).ToList()
                select new GoogleRoute(route.RouteNo, route.Name, route.Color, route.Url, points, path.Distinct().ToList());

            var result = fullRoutes.ToList();
            Debug.Assert(result.Count == routes.Count);
            return result;
        }

        private static List<GoogleRouteSchedule> ParseScheduleCSV(List<StopTimesEntry> stopTimes, List<TripsEntry> trips, ZipArchiveEntry calendarTxt)
        {
            using var calendarCsv = new CsvReader(new StreamReader(calendarTxt.Open()));
            var calendars = calendarCsv.GetRecords<CalendarEntry>().ToList();

            var joinedEntries = stopTimes
                .Join(trips, st => st.TripId, t => t.TripId, (stopTime, trip) => (stopTime, trip))
                .Join(calendars, entry => entry.trip.ServiceId, cal => cal.ServiceId, (entry, calendar) => (entry.stopTime, entry.trip, calendar));

            var aggTimesAtStop = joinedEntries
                .GroupBy(t => (routeNo: t.trip.RouteId, platformTag: t.stopTime.PlatformTag, stopSequence: t.stopTime.StopSequence, days: t.calendar.DaysOfWeek))
                .Select(g => g.OrderBy(t => t.stopTime.ArrivalTime)
                    .Aggregate(new List<TimeSpan>(),
                        (times, t) => { times.Add(t.stopTime.ArrivalTime); return times; },
                        times => (g.Key.routeNo, g.Key.days, stopSchedule: new GoogleStopSchedule(g.Key.platformTag, times))));

            var aggStopsForRoute = aggTimesAtStop
                .GroupBy(t => (t.routeNo, t.days))
                .Select(g => g.Aggregate(new List<GoogleStopSchedule>(),
                    (list, t) => { list.Add(t.stopSchedule); return list; },
                    list => (g.Key.routeNo, g.Key.days, stopSchedules: list)));

            var aggDaysForRoute = aggStopsForRoute
                .GroupBy(t => t.routeNo)
                .Select(g => g.Aggregate(new List<GoogleDaySchedule>(),
                    (list, t) => { list.Add(new GoogleDaySchedule(t.days, t.stopSchedules)); return list; },
                    list => new GoogleRouteSchedule(g.Key, list)))
                .Select(rs => new GoogleRouteSchedule(rs.RouteNo, splitDays(rs.Days)))
                .ToList();

            return aggDaysForRoute;

            List<GoogleDaySchedule> splitDays(List<GoogleDaySchedule> originalDays)
            {
                var daySchedulesBuilder = new List<GoogleDaySchedule>(7)
                {
                    new GoogleDaySchedule(DaysOfWeek.Sunday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Monday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Tuesday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Wednesday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Thursday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Friday, new List<GoogleStopSchedule>()),
                    new GoogleDaySchedule(DaysOfWeek.Saturday, new List<GoogleStopSchedule>()),
                };

                for (int i = 0; i < 7; i++)
                {
                    var day = daySchedulesBuilder[i].Days;
                    foreach (var daySchedule in originalDays)
                    {
                        if ((daySchedule.Days & day) != 0)
                        {
                            var allStopBuilders = daySchedulesBuilder[i].StopSchedules;
                            foreach (var stopSchedule in daySchedule.StopSchedules)
                            {
                                var stopBuilder = allStopBuilders.FirstOrDefault(ss => ss.PlatformTag == stopSchedule.PlatformTag);
                                if (stopBuilder is null)
                                {
                                    stopBuilder = new GoogleStopSchedule(stopSchedule.PlatformTag, new List<TimeSpan>(stopSchedule.Times));
                                    allStopBuilders.Add(stopBuilder);
                                }
                                else
                                {
                                    stopBuilder.Times.AddRange(stopSchedule.Times);
                                    stopBuilder.Times.Sort();
                                    // TODO: god, there's no duplicates, right?
                                }
                            }
                        }
                    }
                }

                // todo: dedupe?
                return daySchedulesBuilder;
            }
        }
    }
}