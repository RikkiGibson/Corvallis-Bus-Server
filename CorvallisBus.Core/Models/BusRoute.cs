using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.Models.GoogleTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents a CTS Route.
    /// </summary>
    public class BusRoute
    {
        [JsonConstructor]
        public BusRoute(
            string routeNo,
            List<int> path,
            string color,
            string url,
            string polyline)
        {
            RouteNo = routeNo;
            Path = path;
            Color = color;
            Url = url;
            Polyline = polyline;
        }

        public static BusRoute Create(ConnexionzRoute connectionzRoute, Dictionary<string, GoogleRoute> googleRoutes)
        {
            var routeNo = connectionzRoute.RouteNo;
            var googleRoute = googleRoutes[routeNo];
            var path = connectionzRoute.Path
                .Select(platform => platform.PlatformId)
                .ToList();

            var url = "https://www.corvallisoregon.gov/cts/page/cts-route-" + routeNo;

            return new BusRoute(routeNo, path, googleRoute.Color, url, connectionzRoute.Polyline);
        }

        /// <summary>
        /// Route Number (e.g. 1, 2, NON, CVA, etc).
        /// </summary>
        [JsonProperty("routeNo")]
        public string RouteNo { get; }

        /// <summary>
        /// List of stop ids on this route, in the order the bus reaches them.
        /// </summary>
        [JsonProperty("path")]
        public List<int> Path { get; }

        /// <summary>
        /// CTS-defined color for this route.
        /// </summary>
        [JsonProperty("color")]
        public string Color { get; }

        /// <summary>
        /// URL to the CTS web page for this route.
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; }

        /// <summary>
        /// Google maps polyline for this route.
        /// </summary>
        [JsonProperty("polyline")]
        public string Polyline { get; }
    }
}