using CorvallisBusCore.Models.Connexionz.GeneratedModels;
using System.Collections.Generic;
using System.Linq;

namespace API.Models.Connexionz
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