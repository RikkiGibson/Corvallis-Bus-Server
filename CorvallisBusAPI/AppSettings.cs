namespace API
{
    /// <summary>
    /// Strongly-typed representation of the API's configurable settings.
    /// 
    /// This is then injected into the controller.
    /// </summary>
    public class AppSettings
    {
        public string BlobStorageConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
        public string RoutesKey { get; set; }
        public string StopsKey { get; set; }
        public string StaticDataKey { get; set; }
        public string PlatformsKey { get; set; }
        public string SchedulesKey { get; set; }
        public string BlobContainerName { get; set; }
    }
}
