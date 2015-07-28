﻿using System.Xml.Linq;

namespace API.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz platform that is pertinent to the app.
    /// </summary>
    public class ConnexionzPlatform
    {
        public ConnexionzPlatform(XElement platform)
        {
            PlatformTag = platform.Attribute("PlatformTag").Value;
            PlatformNo = platform.Attribute("PlatformNo").Value;
            Name = platform.Attribute("Name").Value;

            XElement position = platform.Element("Position");
            double output;
            Lat = double.TryParse(position.Attribute("Lat").Value, out output) ? output : new double?();
            Long = double.TryParse(position.Attribute("Long").Value, out output) ? output : new double?();
        }

        /// <summary>
        /// The 3 digit number which is used in the Connexionz API to get arrival estimates.
        /// </summary>
        public string PlatformTag { get; private set; }

        /// <summary>
        /// The 5 digit number which is printed on bus stop signs in Corvallis.
        /// </summary>
        public string PlatformNo { get; private set; }

        public string Name { get; private set; }

        public double? Lat { get; private set; }

        public double? Long { get; private set; }

    }
}