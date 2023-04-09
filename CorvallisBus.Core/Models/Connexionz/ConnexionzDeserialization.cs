using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CorvallisBus.Core.Models.Connexionz;

public class RoutePositionET
{
    /// <remarks/>
    [XmlElement("Content", typeof(RoutePositionContent))]
    [XmlElement("Platform", typeof(RoutePositionPlatform))]
    public required object[] Items { get; init; }
}

public class RoutePositionPlatform
{
    [XmlElement("Route")]
    public required RoutePositionPlatformRoute[] Route { get; init; }

    [XmlAttribute]
    public required string PlatformTag { get; init; }
}

public class RoutePositionPlatformRouteDestination
{
    [XmlElement("Trip")]
    public required RoutePositionPlatformRouteDestinationTrip[] Trip { get; init; }
}

public class RoutePositionPlatformRouteDestinationTrip
{
    [XmlAttribute]
    public int ETA { get; init; }
}

public class RoutePositionPlatformRoute
{
    [XmlElement("Destination")]
    public required RoutePositionPlatformRouteDestination[] Destination { get; init; }

    [XmlAttribute]
    public required string RouteNo { get; init; }
}

public class RoutePositionContent
{
    [XmlAttribute]
    public required DateTime Expires { get; init; }

    [XmlAttribute]
    public required int MaxArrivalScope { get; init; }
}

public class RoutePattern
{
    [XmlElement(elementName: "Content", typeof(RoutePatternContent))]
    [XmlElement(elementName: "Project", typeof(RoutePatternProject))]
    public required object[] Items { get; init; }
}

public class RoutePatternContent
{
}

public class RoutePatternProject
{
    [XmlElement("Route")]
    public required RoutePatternProjectRoute[] Route { get; init; }
}

public class RoutePatternProjectRoute
{
    [XmlElement("Destination")]
    public required RoutePatternProjectRouteDestination[] Destination { get; init; }

    [XmlAttribute]
    public required string RouteNo { get; init; }
}

public class RoutePatternProjectRouteDestination
{
    public required RoutePatternProjectRouteDestinationPattern Pattern { get; init; }
}

public class RoutePatternProjectRouteDestinationPattern
{
    public required string Mif { get; init; }

    [XmlElement("Platform")]
    public required RoutePatternProjectRouteDestinationPatternPlatform[] Platform { get; init; }

    [XmlAttribute]
    public required string Schedule { get; init; }
}

public class RoutePatternProjectRouteDestinationPatternPlatform
{
    [XmlAttribute]
    public required string PlatformTag { get; init; }

    [XmlAttribute]
    public required string ScheduleAdheranceTimepoint { get; init; }

    [XmlAttribute]
    public required string PlatformNo { get; init; }
}