using System.Xml.Linq;

namespace CorvallisBusDNX.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz platform that is pertinent to the app.
    /// </summary>
    public class ConnexionzPlatform
    {
        public ConnexionzPlatform(XElement platform)
        {
            PlatformTag = int.Parse(platform.Attribute("PlatformTag").Value);
            PlatformNo = int.Parse(platform.Attribute("PlatformNo").Value);
            Name = platform.Attribute("Name").Value;

            XElement position = platform.Element("Position");
            Lat = double.Parse(position.Attribute("Lat").Value);
            Long = double.Parse(position.Attribute("Long").Value);
        }

        /// <summary>
        /// The 3 digit number which is used in the Connexionz CorvallisBusDNX to get arrival estimates.
        /// </summary>
        public int PlatformTag { get; private set; }

        /// <summary>
        /// The 5 digit number which is printed on bus stop signs in Corvallis.
        /// </summary>
        public int PlatformNo { get; private set; }

        public string Name { get; private set; }

        public double Lat { get; private set; }

        public double Long { get; private set; }

    }
}