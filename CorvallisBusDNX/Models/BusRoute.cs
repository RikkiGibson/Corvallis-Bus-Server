using CorvallisBusDNX.Models.Connexionz;
using CorvallisBusDNX.Models.GoogleTransit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBusDNX.Models
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
            Url = googleRoute[RouteNo].Url;
            Polyline = connectionzRoute.Polyline;
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