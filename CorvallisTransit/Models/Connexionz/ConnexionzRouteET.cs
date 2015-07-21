using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models.Connexionz
{
    public class ConnexionzRouteET
    {
        public ConnexionzRouteET(RoutePositionPlatformRoute routePositionPlatformRoute)
        {
            RouteNo = routePositionPlatformRoute.RouteNo;

            // uh oh-- there should be a list of ETAs here
            EstimatedArrivalTimes = new List<int> { routePositionPlatformRoute.Destination.Trip.ETA };
        }

        public string RouteNo { get; private set; }
        public List<int> EstimatedArrivalTimes { get; private set; }
    }
}