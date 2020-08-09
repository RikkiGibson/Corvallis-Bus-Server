using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Represents a route's schedule for particular days of the week in Google Transit.
    /// </summary>
    public record GoogleDaySchedule(
        DaysOfWeek Days,
        List<GoogleStopSchedule> StopSchedules);
}