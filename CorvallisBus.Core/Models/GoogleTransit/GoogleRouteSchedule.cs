using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Represents the schedule for a route taken from Google Transit.
    /// </summary>
    public record GoogleRouteSchedule(
        string RouteNo,
        List<GoogleDaySchedule> Days);
}