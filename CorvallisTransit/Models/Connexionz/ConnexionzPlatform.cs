using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz platform that is pertinent to the app.
    /// </summary>
    public class ConnexionzPlatform
    {
        public ConnexionzPlatform(PlatformsPlatform platformsPlatform)
        {
            PlatformTag = platformsPlatform.PlatformTag;
            PlatformNo = platformsPlatform.PlatformNo;
            Name = platformsPlatform.Name;
            Lat = platformsPlatform.Position?.Lat;
            Long = platformsPlatform.Position?.Long;
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