using System.Collections.Generic;

namespace CorvallisBusCoreNetCore.Models.Connexionz
{
    public class ConnexionzRouteET
    {
        /// <summary>
        /// Empty Constructor for deserialization.
        /// </summary>
        public ConnexionzRouteET() { }

        public ConnexionzRouteET(string routeNo, List<int> etas)
        {
            RouteNo = routeNo;
            EstimatedArrivalTime = etas;
        }

        public string RouteNo { get; private set; }
        public List<int> EstimatedArrivalTime { get; private set; }
    }
}