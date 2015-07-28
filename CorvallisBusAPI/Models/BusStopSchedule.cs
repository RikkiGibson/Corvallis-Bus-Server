using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    /// <summary>
    /// Represents the times that a route will arrive at a particular stop on a particular day.
    /// </summary>
    public class BusStopSchedule
    {
        /// <summary>
        /// The 5-digit number displayed on bus stop signs in Corvallis.
        /// </summary>
        public int Id { get; set; }

        public List<TimeSpan> Times { get; set; }
    }
}
