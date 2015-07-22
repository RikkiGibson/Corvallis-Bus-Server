using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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
        public static void Import()
        {
            List<string> colors = new List<string>();

            using (var archive = new ZipArchive(GetZipFile()))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("routes.txt"))
                    {
                        colors.AddRange(GetRouteColorsFromEntry(entry));
                    }
                    else if (entry.FullName.EndsWith("calendar.txt"))
                    {
                        // get schedule?
                    }
                }
            }
        }

        /// <summary>
        /// Reads a ZipArchive entry as the routes CSV and extracts the route colors.
        /// </summary>
        private static List<string> GetRouteColorsFromEntry(ZipArchiveEntry entry)
        {
            List<string> colors = new List<string>();
            string color;

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

                    // color is the second-to-last entry in the zipped CSV.
                    color = parts[parts.Length - 2];

                    // Trim off the stupid escapes that the StreamReader can't tell are strings.
                    color = color.Substring(1, color.Length - 3);

                    colors.Add(color);
                }
            }

            return colors;
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