using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    public class ShapeEntry
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        [Name("shape_id")]
        public int ShapeId { get; set; }

        [Name("shape_pt_lon")]
        public double ShapePointLon { get; set; }

        [Name("shape_pt_lat")]
        public double ShapePointLat { get; set; }

        [Name("shape_pt_sequence")]
        public int ShapePointSequence { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}