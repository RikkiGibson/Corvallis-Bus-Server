using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models.Connexionz
{
    public class ConnexionzPlatformET
    {
        public ConnexionzPlatformET(RoutePositionPlatform routePositionPlatform)
        {
            PlatformTag = int.Parse(routePositionPlatform.PlatformTag);

            RouteEstimatedArrivals = routePositionPlatform?.Route
                ?.Select(r => new ConnexionzRouteET(r))
                ?.ToList();
        }

        public int PlatformTag { get; private set; }

        public List<ConnexionzRouteET> RouteEstimatedArrivals { get; private set; }
    }
}