using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz route that is pertinent to the app.
    /// </summary>
    public class ConnexionzRoute
    {
        public ConnexionzRoute(RoutePatternProjectRoute routePatternProjectRoute)
        {
            RouteNo = routePatternProjectRoute.RouteNo;

            var longestPattern = routePatternProjectRoute.Destination
                                 .Select(d => d.Pattern)
                                 .Aggregate((p1, p2) => p1.Platform.Length > p2.Platform.Length ? p1 : p2);

            IsActive = longestPattern.Schedule == "Active";

            Polyline = EncodePolyline(GetPoints(longestPattern.Mif));

            var points = GetPoints(longestPattern.Mif);
            // TODO: get the actual data fixed
            if (RouteNo == "4")
            {
                var pointsList = points.ToList();

                // there's a weird point on philomath blvd messing up the route path. just filter it out.
                var lowestIndex = 0;
                for (var i = 1; i < pointsList.Count; i++)
                {
                    if (pointsList[i].Lat < pointsList[lowestIndex].Lat)
                    {
                        lowestIndex = i;
                    }
                }

                pointsList.RemoveAt(lowestIndex);
                points = pointsList;
            }

            Polyline = EncodePolyline(points);

            Path = longestPattern.Platform
                .Where(p => !string.IsNullOrEmpty(p.PlatformNo))
                .Select(p => new ConnexionzRoutePlatform(p))
                .Distinct(ConnexionzRoutePlatformComparer.Instance)
                .ToList();
        }

        public ConnexionzRoute(
            string routeNo,
            List<ConnexionzRoutePlatform> path,
            string polyline,
            bool isActive)
        {
            RouteNo = routeNo;
            Path = path;
            Polyline = polyline;
            IsActive = isActive;
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

            void encodeDiff(int diff)
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
            }
        }

        /// <summary>
        /// The route number, e.g. "1" or "C3".
        /// </summary>
        public string RouteNo { get; }

        /// <summary>
        /// Contains the platform numbers for the platforms that make up this route.
        /// </summary>
        public List<ConnexionzRoutePlatform> Path { get; }

        /// <summary>
        /// Represents the route's path of travel. Encoded as a Google Maps polyline.
        /// Google it if you don't know what that is.
        /// </summary>
        public string Polyline { get; }

        /// <summary>
        /// Indicates whether the route is active in general, i.e. its schedule applies at all.
        /// Several routes are marked as inactive during OSU breaks.
        /// Schedules should only be created for active routes.
        /// </summary>
        public bool IsActive { get; }
    }
}