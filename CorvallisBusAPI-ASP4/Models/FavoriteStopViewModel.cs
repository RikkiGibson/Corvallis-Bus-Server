using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    /// <summary>
    /// A view model intended for consumption by the Corvallis Bus iOS app.
    /// </summary>
    public class FavoriteStopViewModel
    {
        public string StopName { get; set; }
        public int StopId { get; set; }
        public string DistanceFromUser { get; set; }

        /// <summary>
        /// Indicates whether this stop is not a favorite but is
        /// being displayed because it's the nearest stop in town.
        /// </summary>
        public bool IsNearestStop { get; set; }

        public string FirstRouteColor { get; set; }
        public string FirstRouteName { get; set; }
        public string FirstRouteArrivals { get; set; }

        public string SecondRouteColor { get; set; }
        public string SecondRouteName { get; set; }
        public string SecondRouteArrivals { get; set; }
    }
}
