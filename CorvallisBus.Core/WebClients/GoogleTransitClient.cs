using CorvallisBus.Core.Models;
using CorvallisBus.Core.Models.GoogleTransit;
using CorvallisBus.Core.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace CorvallisBus.Core.WebClients
{
    public class GoogleTransitData
    {
        public List<GoogleRoute> Routes { get; }
        public List<GoogleRouteSchedule> Schedules { get; }

        public GoogleTransitData(
            List<GoogleRoute> routes,
            List<GoogleRouteSchedule> schedules)
        {
            Routes = routes;
            Schedules = schedules;
        }
    }

    /// <summary>
    /// Contains the task for importing Route colors from Google Transit.  This task is run once per year(?).
    /// </summary>
    public static class GoogleTransitClient
    {
        /// <summary>
        /// Downloads and interprets the ZIP file CTS uploads for Google.  This is primarily to get route colors and route schedules.
        /// </summary>
        public static GoogleTransitData LoadData()
        {
            List<GoogleRoute> routes = null;
            List<GoogleRouteSchedule> schedules = null;

            using (var archive = new ZipArchive(new MemoryStream(Resources.Google_Transit)))
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

            return new GoogleTransitData(
                routes: routes,
                schedules: schedules
            );
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
                    if (parts[0].Contains("ATS") || parts[0].Contains("PC") || parts[0].Contains("LBL"))
                    {
                        continue;
                    }

                    routes.Add(new GoogleRoute(parts));
                }
            }

            if(!routes.Any(x => x.Name == "C1R"))
            {
                routes.Add(new GoogleRoute("C1R", "9F8E7D"));
            }
            return routes;
        }

        private static Regex s_routePattern = new Regex("^\"((BB_)?[^_]+)_");

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
                        route = s_routePattern.Match(line[0]).Groups[1].Value,
                        stop = line[3],
                        order = int.Parse(line[4]),
                        days = DaysOfWeekUtils.GetDaysOfWeek(line[0]),
                        time = ToTimeSpan(line[1].Replace("\"", string.Empty))
                    });

                // Time to turn some totally flat data into totally structured data.
                var routeDayStopSchedules = flatSchedule.GroupBy(line => new
                    {
                        route = line.route,
                        stop = line.stop,
                        // stops can appear more than once in a route (particularly at the very beginning and very end)
                        // we want to separate each section of the schedule in which the same stop appears.
                        order = line.order,
                        days = line.days
                    })
                    .OrderBy(line => line.Key.order)
                    .Select(grouping => grouping.Aggregate(new List<TimeSpan>(),
                        (times, time) => { times.Add(time.time); return times; },
                        times => new
                        {
                            route = grouping.Key.route,
                            days = grouping.Key.days,
                            stopSchedules = new GoogleStopSchedule(
                                name: grouping.Key.stop,
                                times: times.Distinct().OrderBy(time => time).ToList()
                            )
                        }));

                var routeDaySchedules = routeDayStopSchedules
                    .GroupBy(line => new { route = line.route, days = line.days })
                    .Select(grouping => grouping.Aggregate(new
                    {
                        route = grouping.Key.route,
                        daySchedule = new GoogleDaySchedule(
                            days: grouping.Key.days,
                            stopSchedules: new List<GoogleStopSchedule>()
                        )
                    }, (result, line) => { result.daySchedule.StopSchedules.Add(line.stopSchedules); return result; }));

                // the aristocrats!
                IList<GoogleRouteSchedule> routeSchedules = routeDaySchedules
                    .GroupBy(line => line.route)
                    .Select(grouping => grouping.Aggregate(new GoogleRouteSchedule(
                        routeNo: grouping.Key,
                        days: new List<GoogleDaySchedule>()
                    ), (result, line) => { result.Days.Add(line.daySchedule); return result; })).ToList();

                if(!routeSchedules.Any(x => x.RouteNo == "C1R"))
                {
                    routeSchedules.Add(GetC1RSchedule());
                }
                return routeSchedules.ToList();
            }
        }

        private static GoogleRouteSchedule GetC1RSchedule()
        {
            return new GoogleRouteSchedule(
                routeNo: "C1R",
                days: new List<GoogleDaySchedule>{
                        new GoogleDaySchedule(days: DaysOfWeek.Weekdays,
                                stopSchedules: new List<GoogleStopSchedule>
                                {
                                    new GoogleStopSchedule(
                                        name: "MonroeAve_S_5thSt",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(15,35,0),
                                            new TimeSpan(16,35,0),
                                            new TimeSpan(17,35,0),
                                        }
                                    ),
                                    new GoogleStopSchedule(
                                        name: "ArnoldWay_N_26thSt",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(15,40,0),
                                            new TimeSpan(16,40,0),
                                            new TimeSpan(17,40,0),
                                        }
                                    ),
                                    new GoogleStopSchedule(
                                        name: "WithamHillDr_E_UniversityPl",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(15,45,0),
                                            new TimeSpan(16,45,0),
                                            new TimeSpan(17,45,0),
                                        }
                                    ),
                                    new GoogleStopSchedule(
                                        name: "WithamHillDr_W_ElmwoodDr",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(15,50,0),
                                            new TimeSpan(16,50,0),
                                            new TimeSpan(17,50,0),
                                        }
                                    ),
                                    new GoogleStopSchedule(
                                        name: "KingsBlvd_E_MonroeAve",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(15,55,0),
                                            new TimeSpan(16,55,0),
                                            new TimeSpan(17,55,0),
                                        }
                                    ),
                                    new GoogleStopSchedule(
                                        name: "MonroeAve_S_7thSt",
                                        times: new List<TimeSpan>
                                        {
                                            new TimeSpan(16,0,0),
                                            new TimeSpan(17,0,0),
                                            new TimeSpan(18,0,0),
                                        }
                                    )
                                }
                            )
                        }
                    );
        }
    }
}