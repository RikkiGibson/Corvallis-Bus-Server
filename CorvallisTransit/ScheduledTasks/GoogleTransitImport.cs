using CorvallisTransit.Models;
using CorvallisTransit.Models.GoogleTransit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace CorvallisTransit.Components
{
    /// <summary>
    /// Contains the task for importing Route colors from Google Transit.  This task is run once per year(?).
    /// </summary>
    public static class GoogleTransitImport
    {
        /// <summary>
        /// Downloads and interprets the ZIP file CTS uploads for Google.  This is primarily to get route colors and route schedules.
        /// </summary>
        public static Tuple<List<GoogleRoute>, List<GoogleRouteSchedule>> DoTask()
        {
            List<GoogleRoute> routes = null;
            List<GoogleRouteSchedule> schedules = null;

            using (var archive = new ZipArchive(GetZipFile()))
            {
                var routesEntry = archive.GetEntry("routes.txt");
                if (routesEntry == null)
                {
                    throw new FileNotFoundException("The Google Transit archive did not contain routes.txt.");
                }
                var scheduleEntry = archive.GetEntry("stop_times.txt");
                if (scheduleEntry == null)
                {
                    throw new FileNotFoundException("The Google Transit archive did not contain stop_times.txt.");
                }
                routes = ParseRouteCSV(routesEntry);
                schedules = ParseScheduleCSV(scheduleEntry);
            }

            return Tuple.Create(routes, schedules);
        }

        private static IEnumerable<string> ReadLines(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }

        /// <summary>
        /// Reads a ZipArchive entry as the routes CSV and extracts the route colors.
        /// </summary>
        private static List<GoogleRoute> ParseRouteCSV(ZipArchiveEntry entry)
        {
            var routes = new List<GoogleRoute>();

            using (var reader = new StreamReader(entry.Open()))
            {
                // Ignore the format line
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    var parts = reader.ReadLine().Split(',');

                    // Ignore all routes which aren't part of CTS and thus don't have any real-time data.
                    if (parts[0].Contains("ATS") || parts[0].Contains("PC") ||
                        parts[0].Contains("LBL") || parts[0].Contains("CVA"))
                    {
                        continue;
                    }

                    routes.Add(new GoogleRoute(parts));
                }
            }

            return routes;
        }

        private static Regex m_routePattern = new Regex("^\"((BB_)?[^_]+)_");

        /// <summary>
        /// This gives a time span even if it's over 24 hours-- requires HH:MM or HH:MM:00 format.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private static TimeSpan ToTimeSpan(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                return TimeSpan.Zero;
            }

            var components = time.Split(':');
            return new TimeSpan(int.Parse(components[0]),
                int.Parse(components[1]),
                0);
        }

        private static List<GoogleRouteSchedule> ParseScheduleCSV(ZipArchiveEntry entry)
        {
            using (var reader = new StreamReader(entry.Open()))
            {
                // skip format line
                reader.ReadLine();
                var lines = ReadLines(reader).ToList();
                
                var flatSchedule = lines.Select(line => line.Split(','))
                    .Where(line => !string.IsNullOrWhiteSpace(line[1]))
                    .Select(line => new
                    {
                        route = m_routePattern.Match(line[0]).Groups[1].Value,
                        stop = line[3],
                        days = DaysOfWeekUtils.GetDaysOfWeek(line[0]),
                        time = ToTimeSpan(line[1].Replace("\"", string.Empty))
                    });

                // Time to turn some totally flat data into totally structured data.
                var routeDayStopSchedules = flatSchedule.GroupBy(line => new
                    {
                        route = line.route,
                        stop = line.stop,
                        days = line.days
                    })
                    .Select(grouping => grouping.Aggregate(new
                    {
                        route = grouping.Key.route,
                        days = grouping.Key.days,
                        stopSchedules = new GoogleStopSchedule
                        {
                            Name = grouping.Key.stop,
                            Times = new List<TimeSpan>()
                        }
                    }, (result, line) => { result.stopSchedules.Times.Add(line.time); return result; }));

                // This is not really functional programming.
                foreach (var routeDayStopSchedule in routeDayStopSchedules)
                {
                    // For whatever reason a simple sort in place doesn't work.
                    routeDayStopSchedule.stopSchedules.Times =
                        routeDayStopSchedule.stopSchedules.Times
                            .Where(t => t != TimeSpan.Zero)
                            .Distinct()
                            .OrderBy(time => time)
                            .ToList();
                }

                var routeDaySchedules = routeDayStopSchedules
                    .GroupBy(line => new { route = line.route, days = line.days })
                    .Select(grouping => grouping.Aggregate(new
                    {
                        route = grouping.Key.route,
                        daySchedule = new GoogleDaySchedule
                        {
                            Days = grouping.Key.days,
                            StopSchedules = new List<GoogleStopSchedule>()
                        }
                    }, (result, line) => { result.daySchedule.StopSchedules.Add(line.stopSchedules); return result; }));
                
                // the aristocrats!
                IEnumerable<GoogleRouteSchedule> routeSchedules = routeDaySchedules
                    .GroupBy(line => line.route)
                    .Select(grouping => grouping.Aggregate(new GoogleRouteSchedule
                    {
                        RouteNo = grouping.Key,
                        Days = new List<GoogleDaySchedule>()
                    }, (result, line) => { result.Days.Add(line.daySchedule); return result; }));

                return routeSchedules.ToList();
            }
        }

        /// <summary>
        /// Gets the Google Transit Zipfile as a Memory Stream.
        /// </summary>
        private static Stream GetZipFile()
        {
            string url = "https://dl.dropboxusercontent.com/u/3107589/Google_Transit.zip";

            using (var client = new WebClient())
            {
                var data = client.DownloadData(url);
                return new MemoryStream(data);
            }
        }
    }
}