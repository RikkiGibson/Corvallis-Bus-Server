using System;
using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Represents the times at which a bus stops at a particular stop on a particular day.
    /// </summary>
    public class GoogleStopSchedule
    {
        public string Name { get; }
        public List<TimeSpan> Times { get; }

        public GoogleStopSchedule(
            string name,
            List<TimeSpan> times)
        {
            Name = name;
            Times = times;
        }
    }
}