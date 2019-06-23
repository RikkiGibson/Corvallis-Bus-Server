using System;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Connexionz
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
        }

        public ConnexionzPlatformET(
            int platformTag,
            List<ConnexionzRouteET> routeEstimatedArrivals)
        {
            PlatformTag = platformTag;
            RouteEstimatedArrivals = routeEstimatedArrivals;
        }

        public int PlatformTag { get; }

        public List<ConnexionzRouteET> RouteEstimatedArrivals { get; }
    }
}