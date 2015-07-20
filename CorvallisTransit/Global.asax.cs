using CorvallisTransit.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CorvallisTransit
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // initialize all of our business
            if (TransitClient.Routes == null)
            {
                // this will be what we thread
                Action dataGatheringAction = new Action(() =>
                {
                    lock (TransitClient.StopsLocker)
                    {
                        TransitClient.InitializeAndUpdate();
                    }
                });

                // time our threads, we don't need to make one for each
                // since we'll only be running this periodically
                DateTime start = DateTime.Now;
                dataGatheringAction();
                TimeSpan elapsed = DateTime.Now - start;

                // if somehow it is running already don't need to start
                if (!TransitClient.IsRunning)
                {
                    TransitClient.IsRunning = true;
                    Task.Run(() =>
                    {
                        TimeSpan elapsedInternal = elapsed;
                        DateTime startInternal = start;
                        // if we want to stop it we'll just set IsRunning to false
                        while (TransitClient.IsRunning)
                        {
                            int duration = 60000 - Convert.ToInt32(elapsed.TotalMilliseconds);
                            Thread.Sleep(duration > 0 ? duration : 1);
                            startInternal = DateTime.Now;
                            dataGatheringAction();
                            elapsedInternal = DateTime.Now - startInternal;

                            TransitClient.UpdateClients();
                        }
                    });
                }
            }
        }
    }
}
