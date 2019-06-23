using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models.Connexionz
{
    public class ConnexionzRouteET
    {
        public ConnexionzRouteET(RoutePositionPlatformRoute routePositionPlatformRoute)
        {
            RouteNo = routePositionPlatformRoute.RouteNo;
            EstimatedArrivalTime = routePositionPlatformRoute.Destination[0].Trip.Select(t => t.ETA).ToList();
        }

        public ConnexionzRouteET(
            string routeNo,
            List<int> estimatedArrivalTime)
        {
            RouteNo = routeNo;
            EstimatedArrivalTime = estimatedArrivalTime;
        }

        public string RouteNo { get; }
        public List<int> EstimatedArrivalTime { get; }
    }
}