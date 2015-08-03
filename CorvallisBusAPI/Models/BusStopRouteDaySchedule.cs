using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    /// <summary>
    /// Represents the schedule for a particular route at a particular stop on a particular set of days.
    /// </summary>
    public class BusStopRouteDaySchedule
    {
        public DaysOfWeek Days { get; set; }

        public IEnumerable<TimeSpan> Times { get; set; }

        public IEnumerable<string> DateStrings
        {
            get
            {
                var pacificNow = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Pacific Standard Time");
                return Times.Select(t => DateTime.Today.Add(t))
                    .Where(dt => dt > pacificNow)
                    .Select(dt => dt.ToString("yyyy-MM-dd HH:mm zzz"));
            }
        }
    }
}
