using API.Models;
using API.Models.GoogleTransit;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace API.WebClients
{
    public class GoogleTransitData
    {
        public List<GoogleRoute> Routes { get; set; }
        public List<GoogleRouteSchedule> Schedules { get; set; }
    }

    /// <summary>
    /// Contains the task for importing Route colors from Google Transit.  This task is run once per year(?).
    /// </summary>
    public static class GoogleTransitClient
    {
        private static GoogleTransitData m_googleTransitData;
        public static GoogleTransitData GoogleTransitData
        {
            get
            {
                if (m_googleTransitData == null)
                {
                    m_googleTransitData = DoTask();
                }
                return m_googleTransitData;
            }
        }
        
        /// <summary>
        /// Downloads and interprets the ZIP file CTS uploads for Google.  This is primarily to get route colors and route schedules.
        /// </summary>
        private static GoogleTransitData DoTask()
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

            return new GoogleTransitData
            {
                Routes = routes,
                Schedules = schedules
            };
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
                routes.Add(new GoogleRoute("C1R", "000000", "http://www.corvallisoregon.gov/index.aspx?page=1774"));
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
                        order = line[4],
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
                    .Select(grouping => grouping.Aggregate(new List<TimeSpan>(),
                        (times, time) => { times.Add(time.time); return times; },
                        times => new
                        {
                            route = grouping.Key.route,
                            days = grouping.Key.days,
                            stopSchedules = new GoogleStopSchedule
                            {
                                Name = grouping.Key.stop,
                                Times = times.Distinct().OrderBy(time => time).ToList()
                            }
                        }));

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
                IList<GoogleRouteSchedule> routeSchedules = routeDaySchedules
                    .GroupBy(line => line.route)
                    .Select(grouping => grouping.Aggregate(new GoogleRouteSchedule
                    {
                        RouteNo = grouping.Key,
                        Days = new List<GoogleDaySchedule>()
                    }, (result, line) => { result.Days.Add(line.daySchedule); return result; })).ToList();

                if(!routeSchedules.Any(x => x.RouteNo == "C1R"))
                {
                    routeSchedules.Add(GetC1RSchedule());
                }
                return routeSchedules.ToList();
            }
        }

        /// <summary>
        /// Gets the Google Transit Zipfile as a Memory Stream.
        /// </summary>
        private static Stream GetZipFile()
        {
            string url = "https://dl.dropboxusercontent.com/s/522mkq9rud73r8f/Google_Transit.zip";

            using (var client = new WebClient())
            {
                
                var data = client.DownloadData(url);
                return new MemoryStream(data);
            }
        }

        private static GoogleRouteSchedule GetC1RSchedule()
        {
            return new GoogleRouteSchedule
            {
                RouteNo = "C1R",
                Days = new List<GoogleDaySchedule>{
                        new GoogleDaySchedule {Days = DaysOfWeek.Weekdays,
                                StopSchedules = new List<GoogleStopSchedule>
                                {
                                    new GoogleStopSchedule
                                    {
                                        Name = "MonroeAve_S_5thSt",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,35,0),
                                            new TimeSpan(16,35,0),
                                            new TimeSpan(17,35,0),
                                        }
                                    },
                                    new GoogleStopSchedule
                                    {
                                        Name = "ArnoldWay_N_26thSt",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,40,0),
                                            new TimeSpan(16,40,0),
                                            new TimeSpan(17,40,0),
                                        }
                                    },
                                    new GoogleStopSchedule
                                    {
                                        Name = "WithamHillDr_E_UniversityPl",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,45,0),
                                            new TimeSpan(16,45,0),
                                            new TimeSpan(17,45,0),
                                        }
                                    },
                                    new GoogleStopSchedule
                                    {
                                        Name = "WithamHillDr_W_ElmwoodDr",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,50,0),
                                            new TimeSpan(16,50,0),
                                            new TimeSpan(17,50,0),
                                        }
                                    },
                                    new GoogleStopSchedule
                                    {
                                        Name = "KingsBlvd_E_MonroeAve",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,55,0),
                                            new TimeSpan(16,55,0),
                                            new TimeSpan(17,55,0),
                                        }
                                    },
                                    new GoogleStopSchedule
                                    {
                                        Name = "MonroeAve_S_7thSt",
                                        Times = new List<TimeSpan>
                                        {
                                            new TimeSpan(15,0,0),
                                            new TimeSpan(16,0,0),
                                            new TimeSpan(17,0,0),
                                        }
                                    }
                                }
                            }
                        }
                    };
        }
    }
}