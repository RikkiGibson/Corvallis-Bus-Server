using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    class StopTimesEntry
    {
        [Name("trip_id")]
        public int TripId { get; set; }

        [Name("arrival_time")]
        public TimeSpan ArrivalTime { get; set; }

        [Name("stop_id")]
        public int PlatformTag { get; set; }

        [Name("stop_sequence")]
        public int StopSequence { get; set; }

        [Name("timepoint")]
        public bool Timepoint { get; set; }
    }
}
