using System.Collections.Specialized;

namespace CorvallisBus
{
    /// <summary>
    /// Strongly-typed representation of the API's configurable settings.
    /// 
    /// This is then injected into the controller.
    /// </summary>
    public class AppSettings
    {
        public AppSettings(NameValueCollection terribleThing)
        {
            BlobStorageConnectionString = terribleThing["BlobStorageConnectionString"];
            RedisConnectionString = terribleThing["RedisConnectionString"];
            StaticDataKey = terribleThing["StaticDataKey"];
            PlatformTagsKey = terribleThing["PlatformTagsKey"];
            SchedulesKey = terribleThing["SchedulesKey"];
            BlobContainerName = terribleThing["BlobContainerName"];
        }

        public string BlobStorageConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
        public string StaticDataKey { get; set; }
        public string PlatformTagsKey { get; set; }
        public string SchedulesKey { get; set; }
        public string BlobContainerName { get; set; }
    }
}
