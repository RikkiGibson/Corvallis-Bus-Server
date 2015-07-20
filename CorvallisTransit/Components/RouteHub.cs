using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using CorvallisTransit.Models;
using CorvallisTransit.Controllers;

namespace CorvallisTransit.Components
{
    public class RouteHub : Hub
    {

        public RouteHub() : base()
        {
            TransitClient.UpdateRoute += UpdateRoute;
        }

        public void UpdateRoute(BusRoute route)
        {
            Clients.All.UpdateRoute(route.ClientData);
        }

        public void GetAllRouteInformation()
        {
            var clientData = TransitClient.Routes.Select(rt => rt.ClientData);
            Clients.Caller.UpdateRoutes(clientData);
         
        }

        public void GetRouteInformation(string routeNo)
        {
            Clients.Caller.UpdateRoute(TransitClient.Routes.FirstOrDefault(rt => rt.RouteNo == routeNo).ClientData);
        }
    }
}