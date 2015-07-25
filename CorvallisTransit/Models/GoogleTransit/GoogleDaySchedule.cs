using System.Collections.Generic;

namespace CorvallisTransit.Models.GoogleTransit
{
    /// <summary>
    /// Represents a route's schedule for particular days of the week in Google Transit.
    /// </summary>
    public class GoogleDaySchedule
    {
        public DaysOfWeek Days { get; set; }

        public List<GoogleStopSchedule> StopSchedules { get; set; }
    }
}