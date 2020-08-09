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
    public record FavoriteStopViewModel(
        [property: JsonProperty("stopName")]
        string StopName,

        [property: JsonProperty("stopID")]
        int StopId,

        /// <summary>
        /// The latitude value for the stop (between -90 and 90 degrees).
        /// </summary>
        [property: JsonProperty("lat")]
        double Lat,

        /// <summary>
        /// The longitude value for the stop (between -180 and 180 degrees).
        /// </summary>
        [property: JsonProperty("lng")]
        double Long,

        [property: JsonProperty("distanceFromUser")]
        string DistanceFromUser,

        /// <summary>
        /// Indicates whether this stop is not a favorite but is
        /// being displayed because it's the nearest stop in town.
        /// </summary>
        [property: JsonProperty("isNearestStop")]
        bool IsNearestStop,

        [property: JsonProperty("firstRouteColor")]
        string FirstRouteColor,

        [property: JsonProperty("firstRouteName")]
        string FirstRouteName,

        [property: JsonProperty("firstRouteArrivals")]
        string FirstRouteArrivals,

        [property: JsonProperty("secondRouteColor")]
        string SecondRouteColor,

        [property: JsonProperty("secondRouteName")]
        string SecondRouteName,

        [property: JsonProperty("secondRouteArrivals")]
        string SecondRouteArrivals
        );
}
