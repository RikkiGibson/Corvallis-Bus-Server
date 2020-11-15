using CorvallisBus.Core.Models.Connexionz;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// A bus stop in the Corvallis Transit System. This is analogous to the Platform entity in Connexionz.
    /// </summary>
    /// <param name="Id">
    /// This stop tag is used to get ETAs for the stop from Connexionz.
    /// </param>
    /// <param name="Name">
    /// The name of the stop, for example: &quot;NW Monroe Ave &amp; NW 7th St&quot;.
    /// </param>
    /// <param name="Bearing">
    /// The angle in degrees between this bus stop and the road. This can be treated as
    /// the angle between the positive X axis and the direction of travel for buses at this stop.
    /// </param>
    /// <param name="Lat">
    /// The latitude value for the stop (between -90 and 90 degrees).
    /// </param>
    /// <param name="Long">
    /// The longitude value for the stop (between -180 and 180 degrees).
    /// </param>
    /// <param name="RouteNames">
    /// List of route names which arrive at this stop.
    /// </param>
    public record BusStop(
        [property: JsonProperty("id")]
        int Id,

        [property: JsonProperty("name")]
        string Name,

        [property: JsonProperty("bearing")]
        double Bearing,

        [property: JsonProperty("lat")]
        double Lat,

        [property: JsonProperty("lng")]
        double Long,

        [property: JsonProperty("routeNames")]
        List<string> RouteNames)
    {
        public static string ToDirection(double bearing)
        {
            if (!(bearing >= 0 && bearing <= 360))
                throw new ArgumentException($"Bearing outside of range (0-360): {bearing}");

            if (bearing <= 22.5 || bearing >= 337.5)
                return "E";
            else if (bearing >= 22.5 && bearing <= 67.5)
                return "SE";
            else if (bearing >= 67.5 && bearing <= 112.5)
                return "S";
            else if (bearing >= 112.5 && bearing <= 157.5)
                return "SW";
            else if (bearing >= 157.5 && bearing <= 202.5)
                return "W";
            else if (bearing >= 202.5 && bearing <= 247.5)
                return "NW";
            else if (bearing >= 247.5 && bearing <= 292.5)
                return "N";
            else if (bearing >= 292.5 && bearing <= 337.5)
                return "NE";
            else
                return string.Empty;
        }

        public static BusStop Create(ConnexionzPlatform platform, List<string> routeNames, bool appendDirection)
        {
            return new BusStop(
                Id: platform.PlatformNo,

                Name: platform.CompactName +
                    (appendDirection
                        ? " " + ToDirection(platform.BearingToRoad)
                        : string.Empty),

                Bearing: platform.BearingToRoad,
                Lat: platform.Lat,
                Long: platform.Long,
                RouteNames: routeNames);
        }
    }
}
