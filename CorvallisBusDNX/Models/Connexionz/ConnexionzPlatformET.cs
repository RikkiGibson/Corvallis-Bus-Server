using System;
using System.Collections.Generic;

namespace CorvallisBusDNX.Models.Connexionz
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

        public ConnexionzPlatformET(string platformTag, List<ConnexionzRouteET> routeEstimatedArrivals)
        {
            PlatformTag = int.Parse(platformTag);

            RouteEstimatedArrivals = routeEstimatedArrivals;

            LastUpdated = DateTime.Now;
        }

        public int PlatformTag { get; private set; }

        public DateTime LastUpdated { get; private set; }

        public List<ConnexionzRouteET> RouteEstimatedArrivals { get; private set; }
    }
}