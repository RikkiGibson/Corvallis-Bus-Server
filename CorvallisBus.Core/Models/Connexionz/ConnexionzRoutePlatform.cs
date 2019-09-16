using System.Collections.Generic;

namespace CorvallisBus.Core.Models.Connexionz
{
    /// <summary>
    /// Represents one of the platforms along the path of a particular Connexionz route.
    /// Indicates whether that platform adheres to a schedule for this route.
    /// </summary>
    public struct ConnexionzRoutePlatform
    {
        public ConnexionzRoutePlatform(RoutePatternProjectRouteDestinationPatternPlatform platform)
        {
            PlatformId = int.Parse(platform.PlatformNo is null ? platform.PlatformTag : platform.PlatformNo);

            bool result;
            IsScheduleAdherancePoint = bool.TryParse(platform.ScheduleAdheranceTimepoint, out result) ? result : false;
        }

        public ConnexionzRoutePlatform(
            int platformId,
            bool isScheduleAdherancePoint)
        {
            PlatformId = platformId;
            IsScheduleAdherancePoint = isScheduleAdherancePoint;
        }

        /// <summary>
        /// The 5-digit platform ID. This is what is displayed on bus stop signs.
        /// </summary>
        public int PlatformId { get; }

        /// <summary>
        /// Indicates whether this stop has a schedule defined for it.
        /// This appears to be the most viable way to hook into the Google data.
        /// </summary>
        public bool IsScheduleAdherancePoint { get; }
    }

    /// <summary>
    /// We are going to suppose that when the same platform appears more than once
    /// in a path, it's the only one that should appear in the path.
    /// We are hoping that we don't wipe out a redundant platform where the one we eliminated was the schedule adherance point.
    /// </summary>
    public class ConnexionzRoutePlatformComparer : IEqualityComparer<ConnexionzRoutePlatform>
    {
        private ConnexionzRoutePlatformComparer() { }

        public static ConnexionzRoutePlatformComparer Instance { get; } = new ConnexionzRoutePlatformComparer();

        public bool Equals(ConnexionzRoutePlatform x, ConnexionzRoutePlatform y) =>
            x.PlatformId == y.PlatformId;

        public int GetHashCode(ConnexionzRoutePlatform obj) =>
            obj.GetHashCode();
    }
}