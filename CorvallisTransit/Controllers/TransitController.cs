using CorvallisTransit.Components;
using CorvallisTransit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;

namespace CorvallisTransit.Controllers
{
    /// <summary>
    /// API routes for CTS.
    /// </summary>
    [RoutePrefix("transit")]
    public class TransitController : ApiController
    {
        [HttpGet]
        [Route("static")]
        public async Task<JsonResult<object>> GetStaticData()
        {
            var staticData = await TransitClient.GetStaticData();
            return Json(staticData);
        }

        [Route("{stopIds}")]
        public async Task<JsonResult<object>> GetETAs(string stopIds)
        {
            var splitStopIds = stopIds.Split(',');
            var etas = await TransitClient.GetEtas(splitStopIds);
            return Json(etas);

        }

        [HttpGet]        
        [Route("tasks/google")]
        public void DoGoogleTask()
        {
            var googleRoutes = GoogleTransitImport.DoTask();
            if (googleRoutes.Any())
            {
                StorageManager.UpdateRoutes(googleRoutes);
            }
        }

        [HttpGet]
        [Route("tasks/init")]
        public void Init()
        {
            var googleRoutes = TransitClient.GoogleRoutes.Value.ToDictionary(r => r.ConnexionzName);

            var stops = ConnexionzClient.Platforms.Value.Select(p => new BusStop(p)).ToList();
            StorageManager.Put(stops);

            var routes = ConnexionzClient.Routes.Value.Select(r => new BusRoute(r, googleRoutes)).ToList();
            StorageManager.Put(routes);

            var platformTags = ConnexionzClient.Platforms.Value.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);
            StorageManager.Put(platformTags);
        }
    }
}