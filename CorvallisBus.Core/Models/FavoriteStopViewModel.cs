using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// A view model intended for consumption by the Corvallis Bus iOS app.
    /// </summary>
    public class FavoriteStopViewModel
    {
        [JsonProperty("stopName")]
        public string StopName { get; }

        [JsonProperty("stopID")]
        public int StopId { get; }

        /// <summary>
        /// The latitude value for the stop (between -90 and 90 degrees).
        /// </summary>
        [JsonProperty("lat")]
        public double Lat { get; }

        /// <summary>
        /// The longitude value for the stop (between -180 and 180 degrees).
        /// </summary>
        [JsonProperty("lng")]
        public double Long { get; }

        [JsonProperty("distanceFromUser")]
        public string DistanceFromUser { get; }

        /// <summary>
        /// Indicates whether this stop is not a favorite but is
        /// being displayed because it's the nearest stop in town.
        /// </summary>
        [JsonProperty("isNearestStop")]
        public bool IsNearestStop { get; }

        [JsonProperty("firstRouteColor")]
        public string FirstRouteColor { get; }

        [JsonProperty("firstRouteName")]
        public string FirstRouteName { get; }

        [JsonProperty("firstRouteArrivals")]
        public string FirstRouteArrivals { get; }

        [JsonProperty("secondRouteColor")]
        public string SecondRouteColor { get; }

        [JsonProperty("secondRouteName")]
        public string SecondRouteName { get; }

        [JsonProperty("secondRouteArrivals")]
        public string SecondRouteArrivals { get; }

        public FavoriteStopViewModel(
            string stopName,
            int stopId,
            double lat,
            double @long,
            string distanceFromUser,
            bool isNearestStop,
            string firstRouteColor,
            string firstRouteName,
            string firstRouteArrivals,
            string secondRouteColor,
            string secondRouteName,
            string secondRouteArrivals)
        {
            StopName = stopName;
            StopId = stopId;
            Lat = lat;
            Long = @long;
            DistanceFromUser = distanceFromUser;
            IsNearestStop = isNearestStop;
            FirstRouteColor = firstRouteColor;
            FirstRouteName = firstRouteName;
            FirstRouteArrivals = firstRouteArrivals;
            SecondRouteColor = secondRouteColor;
            SecondRouteName = secondRouteName;
            SecondRouteArrivals = secondRouteArrivals;
        }
    }
}
