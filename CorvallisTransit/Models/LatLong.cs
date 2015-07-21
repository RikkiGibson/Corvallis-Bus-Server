
namespace CorvallisTransit.Models
{
    public struct LatLong
    {
        public LatLong(double lat, double lon)
            : this()
        {
            this.Lat = lat;
            this.Lon = lon;
        }

        public double Lat { get; private set; }
        public double Lon { get; private set; }
    }
}