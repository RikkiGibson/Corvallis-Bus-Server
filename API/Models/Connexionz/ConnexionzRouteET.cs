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
            EstimatedArrivalTime = routePositionPlatformRoute.Destination.Trip.Select(t => t.ETA).FirstOrDefault();
        }

        public string RouteNo { get; set; }
        public int EstimatedArrivalTime { get; set; }
    }
}