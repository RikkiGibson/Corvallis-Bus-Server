using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class FavoriteStop
    {
        public FavoriteStop(int id, string name, List<string> routeNames, double distanceFromUser, bool isNearestStop)
        {
            Id = id;
            Name = name;
            RouteNames = routeNames;
            DistanceFromUser = distanceFromUser;
            IsNearestStop = isNearestStop;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public List<string> RouteNames { get; private set; }
        public double DistanceFromUser { get; private set; }
        public bool IsNearestStop { get; private set; }
    }
}
