using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    public class BusRouteStop
    {
        public string Name { get; set; }
        public BusRoute RouteModel{ get; set;}
        public BusStop StopModel { get; set; }
        public int Eta { get; set; }
        public int StopPosition { get; set; }
        public bool HasSuspiciousTime { get; set; }

    }
}