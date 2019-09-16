using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    class TripsEntry
    {
        [Name("route_id")]
        public string RouteId { get; set; } = null!;

        [Name("service_id")]
        public int ServiceId { get; set; }

        [Name("trip_id")]
        public int TripId { get; set; }
        
        [Name("shape_id")]
        public int ShapeId { get; set; }
    }
}
