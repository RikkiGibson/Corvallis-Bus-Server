using System.Xml.Schema;
using System.Xml.Serialization;

namespace CorvallisBusDNX.Models.Connexionz
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class RoutePattern
    {
        [XmlElement("Content", Type = typeof(RoutePatternContent), Form = XmlSchemaForm.Unqualified)]
        [XmlElement("Project", Type = typeof(RoutePatternProject), Form = XmlSchemaForm.Unqualified)]
        public object[] Items { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RoutePatternContent
    {
        [XmlAttribute]
        public string Expires { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RoutePatternProject
    {
        [XmlElement("Route", Form = XmlSchemaForm.Unqualified)]
        public RoutePatternProjectRoute[] Route { get; set; }

        [XmlAttribute]
        public string ProjectID { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RoutePatternProjectRoute
    {
        [XmlElement("Destination", Form = XmlSchemaForm.Unqualified)]
        public RoutePatternProjectRouteDestination[] Destination { get; set; }

        [XmlAttribute]
        public string RouteNo { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RoutePatternProjectRouteDestination
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public RoutePatternProjectRouteDestinationPattern[] Pattern { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }

    [XmlType(AnonymousType = true)]
    public class RoutePatternProjectRouteDestinationPattern
    {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Mid { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Mif { get; set; }

        [XmlElement("Platform", Form = XmlSchemaForm.Unqualified)]
        public Platform[] Platform { get; set; }

        [XmlAttribute]
        public string RouteTag { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Direction { get; set; }

        [XmlAttribute]
        public string Length { get; set; }

        [XmlAttribute]
        public string Schedule { get; set; }
    }
    
    [XmlType(AnonymousType = true)]
    public class Platform
    {
        [XmlAttribute]
        public string PlatformTag { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string ScheduleAdherenceTimepoint { get; set; }

        [XmlAttribute]
        public string PlatformNo { get; set; }
    }
}
