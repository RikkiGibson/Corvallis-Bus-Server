using System.Collections.Generic;
using System.Linq;

namespace CorvallisBusCoreNetCore.Models.Connexionz
{
    /// <summary>
    /// Intermediate type for the CTS route XML data.
    /// </summary>
    public struct Route
    {
        public Route(string routeNo, IEnumerable<RouteDestination> destinations)
        {
            RouteNo = routeNo;
            Destinations = destinations.ToList();
        }

        public List<RouteDestination> Destinations { get; private set; }

        public string RouteNo { get; private set; }
    }
    
    public struct RouteDestination
    {
        public RouteDestination(string name, RoutePattern pattern)
        {
            Name = name;
            Pattern = pattern;
        }
        
        public RoutePattern Pattern { get; private set; }

        public string Name { get; private set; }
    }

    public struct RoutePattern
    {
        public RoutePattern(string mif, IEnumerable<RoutePlatform> platforms)
        {
            Mif = mif;
            Platforms = platforms.ToList();
        }

        public string Mif { get; private set; }
        
        public List<RoutePlatform> Platforms { get; private set; }
    }
    
    public struct RoutePlatform
    {
        public RoutePlatform(string name, string scheduleAdherenceTimePointText, string platformNo, string platformTag)
        {
            Name = name;
            ScheduleAdherenceTimePointText = scheduleAdherenceTimePointText;
            PlatformNo = platformNo;
            PlatformTag = platformTag;
        }

        public string PlatformTag { get; private set; }
        
        public string Name { get; private set; }
        
        public string ScheduleAdherenceTimePointText { get; private set; }
        
        public string PlatformNo { get; private set; }
    }
}
