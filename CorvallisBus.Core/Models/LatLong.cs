
using System;

namespace CorvallisBus.Core.Models
{
    public readonly struct LatLong
    {
        public LatLong(double lat, double lon)
            : this()
        {
            Lat = lat;
            Lon = lon;
        }

        public double Lat { get; }
        public double Lon { get; }

        public override string ToString()
        {
            var latDirection = Lat < 0 ? "S" : "N";
            var lonDirection = Lon < 0 ? "W" : "E";

            var latAbs = Math.Abs(Lat);
            var lonAbs = Math.Abs(Lon);

            return $"{latAbs}° {latDirection}, {lonAbs}° {lonDirection}";
        }
    }
}