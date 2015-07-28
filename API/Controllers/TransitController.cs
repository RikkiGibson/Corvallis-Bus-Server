﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using API.Components;
using API.Models;

namespace API.Controllers
{
    [Route("[controller]")]
    public class TransitController : Controller
    {
        [HttpGet]
        [Route("static")]
        public async Task<string> GetStaticData()
        {
            var staticData = await TransitClient.GetStaticData();
            return JsonConvert.SerializeObject(staticData);
        }

        /// <summary>
        /// As the name suggests, this gets the ETA information for any number of stop IDs.  The data
        /// is represented as a dictionary, where the keys are the given stop IDs and the values are dictionaries.
        /// These nested dictionaries have route numbers as the keys and integers (ETA) as the values.
        /// </summary>
        [Route("eta/{stopIds}")]
        public async Task<string> GetETAs(string stopIds)
        {
            var splitStopIds = stopIds.Split(',');
            var etas = await TransitClient.GetEtas(splitStopIds.ToList());
            return JsonConvert.SerializeObject(etas);
        }

        /// <summary>
        /// Initiates a data import from Google Transit.
        /// </summary>
        [HttpGet]
        [Route("tasks/google")]
        public string DoGoogleTask()
        {
            var googleRoutes = GoogleTransitImport.DoTask();
            if (googleRoutes.Any())
            {
                StorageManager.UpdateRoutes(googleRoutes);
            }

            return "Google import successful.";
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public string Init()
        {
            var googleRoutes = TransitClient.GoogleRoutes.Value.ToDictionary(r => r.ConnexionzName);

            var stops = ConnexionzClient.Platforms.Value.Select(p => new BusStop(p)).ToList();
            StorageManager.Put(stops);

            var routes = ConnexionzClient.Routes.Value.Select(r => new BusRoute(r, googleRoutes)).ToList();
            StorageManager.Put(routes);

            var platformTags = ConnexionzClient.Platforms.Value.ToDictionary(p => p.PlatformNo, p => p.PlatformTag);
            StorageManager.Put(platformTags);

            CacheManager.ClearCache();

            return "Init job successful.";
        }
    }
}