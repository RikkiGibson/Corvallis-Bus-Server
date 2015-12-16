using CorvallisBusDNX.Models.Connexionz;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBusDNX.Models.Connexionz
{
    public class ConnexionzRouteET
    {
        /// <summary>
        /// Empty Constructor for deserialization.
        /// </summary>
        public ConnexionzRouteET() { }

        public ConnexionzRouteET(RoutePositionPlatformRoute routePositionPlatformRoute)
        {
            RouteNo = routePositionPlatformRoute.RouteNo;
            EstimatedArrivalTime = routePositionPlatformRoute.Destination.Trip.Select(t => t.ETA).ToList();
        }

        public string RouteNo { get; set; }
        public List<int> EstimatedArrivalTime { get; set; }
    }
}