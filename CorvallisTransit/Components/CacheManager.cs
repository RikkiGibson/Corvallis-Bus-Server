using CorvallisTransit.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CorvallisTransit.Components
{
    public static class CacheManager
    {
        private static Lazy<ConnectionMultiplexer> lazyConn = new Lazy<ConnectionMultiplexer>(() =>
        {
            return ConnectionMultiplexer.Connect(ConfigurationManager.AppSettings["RedisCacheConnectionString"]);
        });

        /// <summary>
        /// Connection to the Redis cache.
        /// </summary>
        public static ConnectionMultiplexer Connection => lazyConn.Value;

        // TODO: Figure out a way to fit this into async properly.

        //public static async Task<List<BusRoute>> GetRoutes()
        //{
        //    var database = Connection.GetDatabase();

        //    var json = await database.StringGetAsync("routes");
        //    if (string.IsNullOrEmpty(json))
        //    {
        //        json = await StorageManager.GetRoutesAsync();
        //        database.StringSet("routes", json);
        //    }

        //    return await Task.Run(() => ;
        //}
    }
}