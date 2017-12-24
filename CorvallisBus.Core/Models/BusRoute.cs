using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a CTS Route.
    /// </summary>
    public class BusRoute
    {
        /// <summary>
        /// Empty Constructor for JSON Deserialization.
        /// </summary>
        public BusRoute() { }

        public BusRoute(ConnexionzRoute connectionzRoute, Dictionary<string, GoogleRoute> googleRoute)
        {
            RouteNo = connectionzRoute.RouteNo;

            Path = connectionzRoute.Path
                .Select(platform => platform.PlatformId)
                .ToList();

            Color = googleRoute[RouteNo].Color;
            Url = LookupUrl(RouteNo);
            Polyline = connectionzRoute.Polyline;
        }

        public static string LookupUrl(string routeName)
        {
            string suffix;
            if (routeName == "NON")
                suffix = "night-owl-north";
            else if (routeName == "NOSE")
                suffix = "night-owl-southeast";
            else if (routeName == "NOSW")
                suffix = "night-owl-southwest";
            else
                suffix = routeName.ToLower();

            return "https://www.corvallisoregon.gov/cts/page/cts-route-" + suffix;
        }

        /// <summary>
        /// Route Number (e.g. 1, 2, NON, CVA, etc).
        /// </summary>
        [JsonProperty("routeNo")]
        public string RouteNo { get; set; }

        /// <summary>
        /// List of stop ids on this route, in the order the bus reaches them.
        /// </summary>
        [JsonProperty("path")]
        public List<int> Path { get; set; }

        /// <summary>
        /// CTS-defined color for this route.
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; set; }

        /// <summary>
        /// URL to the CTS web page for this route.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Google maps polyline for this route.
        /// </summary>
        [JsonProperty("polyline")]
        public string Polyline { get; set; }
    }
}