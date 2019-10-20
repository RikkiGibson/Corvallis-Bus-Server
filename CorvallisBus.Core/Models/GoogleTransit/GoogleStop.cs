using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    public class GoogleStop
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Name("stop_id")]
        public int PlatformTag { get; set; }

        [Name("stop_code")]
        public int? StopId { get; set; } // prefer stopid if available, otherwise platformtag is ok

        [Name("stop_name")]
        public string Name { get; set; }

        [Name("stop_lat")]
        public double Lat { get; set; }

        [Name("stop_lon")]
        public double Lon { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}