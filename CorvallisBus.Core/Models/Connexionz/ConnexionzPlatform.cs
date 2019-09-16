using System.Xml.Linq;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz platform that is pertinent to the app.
    /// </summary>
    public class ConnexionzPlatform
    {
        public ConnexionzPlatform(XElement platform)
        {
            PlatformTag = int.Parse(platform.Attribute("PlatformTag").Value);

            var platformNoAttr = platform.Attribute("PlatformNo");

            // TODO: use a default PlatformNo or filter out such platforms?
            PlatformNo = platformNoAttr is null ? 0 : int.Parse(platformNoAttr.Value);

            // Almost all stops except for places like HP and CVHS have this attribute.
            // It's reasonable to default to having those stops point in the positive X axis direction.
            double.TryParse(platform.Attribute("BearingToRoad")?.Value, out double bearing);
            BearingToRoad = bearing;

            Name = platform.Attribute("Name").Value;

            XElement position = platform.Element("Position");
            Lat = double.Parse(position.Attribute("Lat").Value);
            Long = double.Parse(position.Attribute("Long").Value);
        }

        public ConnexionzPlatform(
            int platformTag,
            int platformNo,
            double bearingToRoad,
            string name,
            double lat,
            double @long)
        {
            PlatformTag = platformTag;
            PlatformNo = platformNo;
            BearingToRoad = bearingToRoad;
            Name = name;
            Lat = lat;
            Long = @long;
        }

        /// <summary>
        /// The 3 digit number which is used in the Connexionz API to get arrival estimates.
        /// </summary>
        public int PlatformTag { get; }

        /// <summary>
        /// The 5 digit number which is printed on bus stop signs in Corvallis.
        /// </summary>
        public int PlatformNo { get; }

        /// <summary>
        /// The angle in degrees between this bus stop and the road. This can be treated as
        /// the angle between the positive X axis and the direction of travel for buses at this stop.
        /// </summary>
        public double BearingToRoad { get; }

        public string Name { get; }

        public double Lat { get; }

        public double Long { get; }

    }
}