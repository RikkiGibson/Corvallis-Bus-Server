using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary>
    /// Represents all the information about a Connexionz platform that is pertinent to the app.
    /// </summary>
    public record ConnexionzPlatform(
        /// <summary>
        /// The 3 digit number which is used in the Connexionz API to get arrival estimates.
        /// </summary>
        int PlatformTag,

        /// <summary>
        /// The 5 digit number which is printed on bus stop signs in Corvallis.
        /// </summary>
        int PlatformNo,

        /// <summary>
        /// The angle in degrees between this bus stop and the road. This can be treated as
        /// the angle between the positive X axis and the direction of travel for buses at this stop.
        /// </summary>
        double BearingToRoad,

        string Name,

        string CompactName,

        double Lat,

        double Long)
    {
        public static ConnexionzPlatform Create(XElement platform)
        {
            var platformNoAttr = platform.Attribute("PlatformNo");

            // Almost all stops except for places like HP and CVHS have this attribute.
            // It's reasonable to default to having those stops point in the positive X axis direction.
            double.TryParse(platform.Attribute("BearingToRoad")?.Value, out double bearing);

            var name = platform.Attribute("Name").Value;

            XElement position = platform.Element("Position");

            return new ConnexionzPlatform(
                PlatformTag: int.Parse(platform.Attribute("PlatformTag").Value),
                PlatformNo: int.Parse(platformNoAttr.Value),
                BearingToRoad: bearing,
                Name: name,
                CompactName: GetCompactName(name),
                Lat: double.Parse(position.Attribute("Lat").Value),
                Long: double.Parse(position.Attribute("Long").Value));
        }

        private static string GetCompactName(string name)
        {
            var compactName = name;
            compactName = Regex.Replace(input: compactName, pattern: @"\bSouthwest\b", "SW");
            compactName = Regex.Replace(input: compactName, pattern: @"\bSouthwest\b", "SW");
            compactName = Regex.Replace(input: compactName, pattern: @"\bNorthwest\b", "NW");
            compactName = Regex.Replace(input: compactName, pattern: @"\bSoutheast\b", "SE");
            compactName = Regex.Replace(input: compactName, pattern: @"\bNortheast\b", "NE");
            compactName = Regex.Replace(input: compactName, pattern: @"\bBoulevard\b", "Blvd");
            compactName = Regex.Replace(input: compactName, pattern: @"\bBoulevar\b", "Blvd");
            compactName = Regex.Replace(input: compactName, pattern: @"\bBoulev\b", "Blvd");
            compactName = Regex.Replace(input: compactName, pattern: @"\bBo\b", "Blvd");
            compactName = Regex.Replace(input: compactName, pattern: @"\bDrive\b", "Dr");
            compactName = Regex.Replace(input: compactName, pattern: @"\bDriv\b", "Dr");
            compactName = Regex.Replace(input: compactName, pattern: @"\bD\b", "Dr");
            compactName = Regex.Replace(input: compactName, pattern: @"\bStreet\b", "St");
            compactName = Regex.Replace(input: compactName, pattern: @"\bStre\b", "St");
            compactName = Regex.Replace(input: compactName, pattern: @"\bAvenue\b", "Ave");
            compactName = Regex.Replace(input: compactName, pattern: @"\bApartments\b", "Apts");

            return compactName;
        }
    }
}