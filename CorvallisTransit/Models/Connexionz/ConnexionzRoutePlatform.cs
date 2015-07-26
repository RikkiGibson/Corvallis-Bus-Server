namespace CorvallisTransit.Models.Connexionz
{
    /// <summary>
    /// Represents one of the platforms along the path of a particular Connexionz route.
    /// Indicates whether that platform adheres to a schedule for this route.
    /// </summary>
    public struct ConnexionzRoutePlatform
    {
        public ConnexionzRoutePlatform(Platform platform)
        {
            PlatformId = int.Parse(platform.PlatformNo);

            bool result;
            IsScheduleAdherancePoint = bool.TryParse(platform.ScheduleAdheranceTimepoint, out result) ? result : false;
        }

        /// <summary>
        /// The 5-digit platform ID. This is what is displayed on bus stop signs.
        /// </summary> 
        public int PlatformId { get; private set; }

        /// <summary>
        /// Indicates whether this stop has a schedule defined for it.
        /// This appears to be the most viable way to hook into the Google data.
        /// </summary>
        public bool IsScheduleAdherancePoint { get; private set; }
    }
}