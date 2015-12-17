using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBusCoreNetCore.Models
{
    /// <summary>
    /// Represents the schedule for a particular route at a particular stop on a particular set of days.
    /// </summary>
    public class BusStopRouteDaySchedule
    {
        public DaysOfWeek Days { get; set; }
        public List<TimeSpan> Times { get; set; }
    }
}
