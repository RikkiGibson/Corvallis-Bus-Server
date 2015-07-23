using CorvallisTransit.Components;
using CorvallisTransit.Models.Connexionz;
using CorvallisTransit.Models.GoogleTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
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
            Path = connectionzRoute.Path;
            Color = googleRoute[RouteNo].Color;
            Url = googleRoute[RouteNo].Url;
            Polyline = connectionzRoute.Polyline;
        }

        /// <summary>
        /// Route Number (e.g. 1, 2, NON, CVA, etc).
        /// </summary>
        public string RouteNo { get; set; }

        /// <summary>
        /// List of stop ids on this route, in the order the bus reaches them.
        /// </summary>
        public List<int> Path { get; set; }

        /// <summary>
        /// CTS-defined color for this route.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// URL to the CTS web page for this route.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Google maps polyline for this route.
        /// </summary>
        public string Polyline { get; set; }
    }
}