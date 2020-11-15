using System;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary>
    /// An association between a Connexionz Platform Tag and a list of arrivals for the stop which corresponds to that tag.
    /// </summary>
    public record ConnexionzPlatformET(
        int PlatformTag,
        List<ConnexionzRouteET>? RouteEstimatedArrivals)
    {
        public ConnexionzPlatformET(RoutePositionPlatform routePositionPlatform)
            : this(
                int.Parse(routePositionPlatform.PlatformTag),
                routePositionPlatform.Route
                    ?.Select(r => new ConnexionzRouteET(r))
                            ?.ToList())
        {
        }
    }
}