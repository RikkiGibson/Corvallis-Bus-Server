using System.Collections.Generic;

namespace API.Models
{
    public class BusStopRouteSchedule
    {
        public string RouteNo { get; set; }
        public IEnumerable<BusStopRouteDaySchedule> DaySchedules { get; set; }
    }
}