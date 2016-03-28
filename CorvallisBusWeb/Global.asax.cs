using API;
using API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Routing;

namespace CorvallisBusWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ScheduleTransitDataRefresh();
        }

        const string INIT_TRANSIT_DATA = "InitTransitData";
        private void ScheduleTransitDataRefresh()
        {
            // Make this event recur at the nearest 4 AM
            DateTime nextMorning = DateTime.Now.Hour >= 4
                ? DateTime.Today.AddDays(1).AddHours(4)
                : DateTime.Today.AddHours(4);
        
            // Recreate static data cache when HttpRuntime cache expires
            HttpRuntime.Cache.Insert(key: INIT_TRANSIT_DATA, value: INIT_TRANSIT_DATA, dependencies: null,
                absoluteExpiration: nextMorning, slidingExpiration: TimeSpan.Zero, priority: CacheItemPriority.Default, onRemoveCallback: OnCacheItemRemoved);
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if (key == INIT_TRANSIT_DATA)
            {
                // TODO: factor init out into manager
                var controller = new TransitApiController();
                controller.Init();

                ScheduleTransitDataRefresh();
            }
        }

    }
}
