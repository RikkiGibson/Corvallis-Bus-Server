using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    /// <summary>
    /// Extensions for generated RoutePositions
    /// </summary>
    public partial class RoutePosition
    {
        public RoutePositionContent ContentInfo
        {
            get
            {
                return Items.First() as RoutePositionContent;
            }
        }

        public RoutePositionPlatform GetPlatform(BusRouteStop stop)
        {
            return Items.Length > 1 ? 
                Items.Skip(1)
                    .Select(i => i as RoutePositionPlatform)
                    .FirstOrDefault(s => s.Route != null && s.Route.Any(r => r.RouteNo == stop.RouteModel.RouteNo)) :
                null;
        }
    }
}