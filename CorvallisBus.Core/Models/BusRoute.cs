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
    public record BusRoute(
        /// <summary>
        /// Route Number (e.g. 1, 2, NON, CVA, etc).
        /// </summary>
        [JsonProperty("routeNo")]
        string RouteNo,

        /// <summary>
        /// List of stop ids on this route, in the order the bus reaches them.
        /// </summary>
        [JsonProperty("path")]
        List<int> Path,

        /// <summary>
        /// CTS-defined color for this route.
        /// </summary>
        [JsonProperty("color")]
        string Color,

        /// <summary>
        /// URL to the CTS web page for this route.
        /// </summary>
        [JsonProperty("url")]
        string Url,

        /// <summary>
        /// Google maps polyline for this route.
        /// </summary>
        [JsonProperty("polyline")]
        string Polyline)
    {
        public static BusRoute Create(ConnexionzRoute connectionzRoute, Dictionary<string, GoogleRoute> googleRoutes)
        {
            var routeNo = connectionzRoute.RouteNo;
            var googleRoute = googleRoutes[routeNo];
            var path = connectionzRoute.Path
                .Select(platform => platform.PlatformId)
                .ToList();

            var routeUrlSuffix = routeNo switch
            {
                "NON" => "night-owl-north",
                "NOSE" => "night-owl-southeast",
                "NOSW" => "night-owl-southwest",
                _ => routeNo
            };
            var url = "https://www.corvallisoregon.gov/cts/page/cts-route-" + routeUrlSuffix;

            return new BusRoute(routeNo, path, googleRoute.Color, url, connectionzRoute.Polyline);
        }
    }
}