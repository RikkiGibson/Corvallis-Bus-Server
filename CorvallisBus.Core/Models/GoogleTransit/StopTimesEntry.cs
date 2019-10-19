using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    struct StopTimesEntry
    {
        private class ArrivalTimeConverter : ITypeConverter
        {
            public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                var parts = text.Split(':');
                if (parts.Length != 3)
                {
                    throw new ArgumentException($"Bad ArrivalTime format: {text}");
                }

                var (hours, minutes, seconds) = (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
                return new TimeSpan(hours, minutes, seconds);
            }

            public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            {
                throw new NotImplementedException();
            }
        }

        [Name("trip_id")]
        public int TripId { get; set; }

        [Name("arrival_time"), TypeConverter(typeof(ArrivalTimeConverter))]
        public TimeSpan ArrivalTime { get; set; }

        [Name("stop_id")]
        public int PlatformTag { get; set; }

        [Name("stop_sequence")]
        public int StopSequence { get; set; }

        [Name("timepoint")]
        public bool Timepoint { get; set; }
    }
}
