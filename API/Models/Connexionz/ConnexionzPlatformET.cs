using System;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Connexionz
{
    /// <summary>
    /// An association between a Connexionz Platform Tag and a list of arrivals for the stop which corresponds to that tag.
    /// </summary>
    public class ConnexionzPlatformET
    {
        /// <summary>
        /// Empty constructor for Deserialization.
        /// </summary>
        public ConnexionzPlatformET() { }

        public ConnexionzPlatformET(RoutePositionPlatform routePositionPlatform)
        {
            PlatformTag = int.Parse(routePositionPlatform.PlatformTag);

            RouteEstimatedArrivals = routePositionPlatform?.Route
                ?.Select(r => new ConnexionzRouteET(r))
                ?.ToList();

            LastUpdated = DateTime.Now;
        }

        public int PlatformTag { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<ConnexionzRouteET> RouteEstimatedArrivals { get; set; }
    }
}