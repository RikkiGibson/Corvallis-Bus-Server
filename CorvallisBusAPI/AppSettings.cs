using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
{
    public class AppSettings
    {
        public string BlobStorageConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
        public string RoutesKey { get; set; }
        public string StopsKey { get; set; }
        public string PlatformsKey { get; set; }
        public string SchedulesKey { get; set; }
        public string BlobContainerName { get; set; }
    }
}
