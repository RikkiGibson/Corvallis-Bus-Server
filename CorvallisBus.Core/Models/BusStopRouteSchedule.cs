using System;
using System.Collections.Generic;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the schedule for a particular route at a particular stop.
    /// </summary>
    public class BusStopRouteSchedule
    {
        public string RouteNo { get; }
        public List<BusStopRouteDaySchedule> DaySchedules { get; }

        public BusStopRouteSchedule(
            string routeNo,
            List<BusStopRouteDaySchedule> daySchedules)
        {
            RouteNo = routeNo;
            DaySchedules = daySchedules;
        }
    }
}