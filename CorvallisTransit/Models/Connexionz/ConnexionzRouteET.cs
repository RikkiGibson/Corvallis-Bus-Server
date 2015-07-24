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
            // Either navigate the XML document manually or regenerate the model:
            // https://msdn.microsoft.com/en-us/library/x6c1kb0s(v=vs.110).aspx
            EstimatedArrivalTime = routePositionPlatformRoute.Destination.Trip.Select(t => t.ETA).Single();
        }

        public string RouteNo { get; private set; }
        public int EstimatedArrivalTime { get; private set; }
    }
}