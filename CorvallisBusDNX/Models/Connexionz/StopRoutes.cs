using System;
using System.Xml.Serialization;

namespace CorvallisBusDNX.Models.Connexionz
{
    [XmlRoot(ElementName = "RoutePositionET", Namespace = "urn:connexionz-co-nz", IsNullable = false)]
    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePosition
    {
        [XmlElement("Content", typeof(RoutePositionContent))]
        [XmlElement("Platform", typeof(RoutePositionPlatform))]
        public object[] Items { get; set; }
    } 

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePositionContent
    {
        [XmlAttribute]
        public DateTime Expires { get; set; }

        [XmlIgnore]
        public bool ExpiresSpecified { get; set; }

        [XmlAttribute]
        public int MaxArrivalScope { get; set; }

        [XmlIgnore]
        public bool MaxArrivalScopeSpecified { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePositionPlatform
    {
        [XmlElement("Route")]
        public RoutePositionPlatformRoute[] Route { get; set; }

        [XmlAttribute]
        public string PlatformTag { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePositionPlatformRoute
    {
        [XmlElement("Destination")]
        public RoutePositionPlatformRouteDestination Destination { get; set; }

        [XmlAttribute]
        public string RouteNo { get; set; }

        [XmlIgnore]
        public bool RouteNoSpecified { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePositionPlatformRouteDestination
    {
        [XmlElement("Trip")]
        public RoutePositionPlatformRouteDestinationTrip[] Trip { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true, Namespace = "urn:connexionz-co-nz")]
    public class RoutePositionPlatformRouteDestinationTrip
    {
        [XmlAttribute]
        public int ETA { get; set; }

        [XmlIgnore]
        public bool ETASpecified { get; set; }
    }
}