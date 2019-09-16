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

        public static async Task<BusRoute> Create(ConnexionzRoute connectionzRoute, Dictionary<string, GoogleRoute> googleRoute, Func<string, Task<string>> urlResolver)
        {
            var routeNo = connectionzRoute.RouteNo;
            var url = await LookupUrl(routeNo, urlResolver);
            var path = connectionzRoute.Path
                .Select(platform => platform.PlatformId)
                .ToList();

            return new BusRoute(routeNo, path, googleRoute[routeNo].Color, url, connectionzRoute.Polyline);
        }

        public static async Task<string> LookupUrl(string routeName, Func<string, Task<string>> urlResolver)
        {
            var suffix = routeName.ToLower();
            var url = "https://www.corvallisoregon.gov/cts/page/cts-route-" + suffix;
            return await urlResolver(url);
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