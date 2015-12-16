using System;
using System.Xml.Serialization;

namespace CorvallisBusDNX.Models.Connexionz
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(IsNullable = false)]
    public class Platforms
    {
        [XmlElement("Content", typeof(PlatformsContent))]
        [XmlElement("Platform", typeof(PlatformsPlatform))]
        public object[] Items { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class PlatformsContent
    {
        [XmlAttribute]
        public DateTime Expires { get; set; }

        [XmlIgnore]
        public bool ExpiresSpecified { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class PlatformsPlatform
    {
        [XmlElement("Position")]
        public PlatformsPlatformPosition Position { get; set; }

        [XmlAttribute]
        public string PlatformTag { get; set; }

        [XmlAttribute]
        public string PlatformNo { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public double BearingToRoad { get; set; }

        [XmlIgnore]
        public bool BearingToRoadSpecified { get; set; }

        [XmlAttribute]
        public string RoadName { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class PlatformsPlatformPosition
    {
        [XmlAttribute]
        public double Lat { get; set; }

        [XmlIgnore]
        public bool LatSpecified { get; set; }

        [XmlAttribute]
        public double Long { get; set; }

        [XmlIgnore]
        public bool LongSpecified { get; set; }
    }
}
