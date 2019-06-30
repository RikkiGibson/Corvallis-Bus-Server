﻿
namespace CorvallisBus.Core.Models
{
    public struct LatLong
    {
        public LatLong(double lat, double lon)
            : this()
        {
            Lat = lat;
            Lon = lon;
        }

        public double Lat { get; }
        public double Lon { get; }
    }
}