using System;
using System.Collections.Generic;

namespace CorvallisTransit.Models.GoogleTransit
{
    /// <summary>
    /// Represents the times at which a bus stops at a particular stop on a particular day.
    /// </summary>
    public class GoogleStopSchedule
    {
        public string Name { get; set; }
        public List<TimeSpan> Times { get; set; }
    }
}