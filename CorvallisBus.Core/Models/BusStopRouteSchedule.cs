using System.Collections.Generic;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the schedule for a particular route at a particular stop.
    /// </summary>
    public record BusStopRouteSchedule(
        string RouteNo,
        List<BusStopRouteDaySchedule> DaySchedules);
}