using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    public class FavoriteStop
    {
        public FavoriteStop(int id, string name, List<string> routeNames, double lat, double lng, double distanceFromUser, bool isNearestStop)
        {
            Id = id;
            Name = name;
            RouteNames = routeNames;
            Lat = lat;
            Long = lng;
            DistanceFromUser = distanceFromUser;
            IsNearestStop = isNearestStop;
        }

        public int Id { get; }
        public string Name { get; }
        public List<string> RouteNames { get; }
        public double Lat { get; }
        public double Long { get; }
        public double DistanceFromUser { get; }
        public bool IsNearestStop { get; }
    }
}
