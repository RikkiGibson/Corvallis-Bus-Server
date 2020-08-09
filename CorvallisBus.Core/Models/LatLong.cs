
namespace CorvallisBus.Core.Models
{
    public struct LatLong
    {
        public LatLong(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public double Lat { get; }
        public double Lon { get; }
    }
}