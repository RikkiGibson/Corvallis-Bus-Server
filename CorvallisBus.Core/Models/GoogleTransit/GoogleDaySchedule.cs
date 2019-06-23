using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Represents a route's schedule for particular days of the week in Google Transit.
    /// </summary>
    public class GoogleDaySchedule
    {
        public DaysOfWeek Days { get; }

        public List<GoogleStopSchedule> StopSchedules { get; }

        public GoogleDaySchedule(
            DaysOfWeek days,
            List<GoogleStopSchedule> stopSchedules)
        {
            Days = days;
            StopSchedules = stopSchedules;
        }
    }
}