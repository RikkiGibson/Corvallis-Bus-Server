using System;
using System.Collections.Generic;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    /// <summary>
    /// Represents the times at which a bus stops at a particular stop on a particular day.
    /// </summary>
    public class GoogleStopSchedule
    {
        public int PlatformTag { get; }
        public List<TimeSpan> Times { get; }

        public GoogleStopSchedule(
            int platformTag,
            List<TimeSpan> times)
        {
            PlatformTag = platformTag;
            Times = times;
        }
    }
}