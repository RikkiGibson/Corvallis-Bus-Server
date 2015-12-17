using CorvallisBusDNX.Models.Connexionz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CorvallisBusDNX.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz route that is pertinent to the app.
    /// </summary>
    public class ConnexionzRoute
    {
        public ConnexionzRoute(string routeNo, 
            IEnumerable<RouteDestination> destinations, 
            string mif, 
            IEnumerable<RoutePattern> platforms)
        {
            RouteNo = routeNo;

            // Some routes have multiple paths. Let's just take whichever path is longest.
            var longestPattern = destinations
                                 .Select(d => d.Pattern)
                                 .Aggregate((p1, p2) => p1.Platforms.Count > p2.Platforms.Count ? p1 : p2);

            Polyline = EncodePolyline(GetPoints(longestPattern.Mif));

            Path = longestPattern.Platforms
                   .Select(p => new ConnexionzRoutePlatform(p.PlatformNo, p.ScheduleAdherenceTimePointText))
                   .Distinct(ConnexionzRoutePlatformComparer.Instance)
                   .ToList();
        }

        public ConnexionzRoute(Route routePatternProjectRoute)
        {
            RouteNo = routePatternProjectRoute.RouteNo;

            // Some routes have multiple paths. Let's just take whichever path is longest.
            var longestPattern = routePatternProjectRoute.Destinations
                                 .Select(d => d.Pattern)
                                 .Aggregate((p1, p2) => p1.Platforms.Count > p2.Platforms.Count ? p1 : p2);

            Polyline = EncodePolyline(GetPoints(longestPattern.Mif));

            Path = longestPattern.Platforms
                   .Select(p => new ConnexionzRoutePlatform(p.PlatformNo, p.ScheduleAdherenceTimePointText))
                   .Distinct(ConnexionzRoutePlatformComparer.Instance)
                   .ToList();
        }

        public static IEnumerable<LatLong> GetPoints(string mif)
        {
            var matches = Regex.Matches(mif, @"-?\d+\.\d+");

            for (int i = 0; i < matches.Count - 1; i += 2)
            {
                yield return new LatLong(double.Parse(matches[i + 1].Value), double.Parse(matches[i].Value));
            }
        }

        /// <summary>
        /// Encodes a collection of LatLongs into GoogleMaps-style polylines.
        /// </summary>
        public static string EncodePolyline(IEnumerable<LatLong> points)
        {
            var str = new StringBuilder();

            Action<int> encodeDiff = diff =>
            {
                int shifted = diff << 1;
                if (diff < 0)
                    shifted = ~shifted;

                int rem = shifted;
                while (rem >= 0x20)
                {
                    str.Append((char)((0x20 | (rem & 0x1f)) + 63));
                    rem >>= 5;
                }

                str.Append((char)(rem + 63));
            };

            int lastLat = 0;
            int lastLng = 0;
            foreach (var point in points)
            {
                int lat = (int)Math.Round(point.Lat * 1E5);
                int lng = (int)Math.Round(point.Lon * 1E5);

                encodeDiff(lat - lastLat);
                encodeDiff(lng - lastLng);

                lastLat = lat;
                lastLng = lng;
            }

            return str.ToString();
        }

        /// <summary>
        /// The route number, e.g. "1" or "C3".
        /// </summary>
        public string RouteNo { get; private set; }

        /// <summary>
        /// Contains the platform numbers for the platforms that make up this route.
        /// </summary>
        public List<ConnexionzRoutePlatform> Path { get; private set; }

        /// <summary>
        /// Represents the route's path of travel. Encoded as a Google Maps polyline.
        /// Google it if you don't know what that is.
        /// </summary>
        public string Polyline { get; private set; }
    }
}