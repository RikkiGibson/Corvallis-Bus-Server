using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CorvallisTransit.Models.Connexionz
{

    /// <summary>
    /// Represents all the information about a Connexionz route that is pertinent to the app.
    /// </summary>
    public class ConnexionzRoute
    {
        public ConnexionzRoute(RoutePatternProjectRoute routePatternProjectRoute)
        {
            RouteNo = routePatternProjectRoute.RouteNo;

            // Some routes have multiple paths. Let's just take whichever path is longest.
            var longestPattern = routePatternProjectRoute.Destination
                .Select(d => d.Pattern.First())
                .Aggregate((p1, p2) => p1.Platform.Length > p2.Platform.Length ? p1 : p2);

            var matches = Regex.Matches(longestPattern.Mif, @"-?\d+\.\d+");

            Polyline = new List<LatLong>(matches.Count / 2);
            for (int i = 0; i < matches.Count; i += 2)
            {
                var latlong = new LatLong(double.Parse(matches[i].Value), double.Parse(matches[i+1].Value));
                Polyline.Add(latlong);
            }

            Path = longestPattern.Platform
                .Select(p => int.Parse(p.PlatformNo))
                .ToList();
        }

        /// <summary>
        /// The route number, e.g. "1" or "C3".
        /// </summary>
        public string RouteNo { get; private set; }

        /// <summary>
        /// Contains the platform numbers for the platforms that make up this route.
        /// </summary>
        public List<int> Path { get; private set; }

        /// <summary>
        /// Represents the route's path of travel.
        /// </summary>
        public List<LatLong> Polyline { get; private set; }
    }
}