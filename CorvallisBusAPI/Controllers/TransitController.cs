using System.Linq;
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
            // TODO: ideally, there's no construction involved in our static payloads.
            // we have the JSON string in a cache or blob and that string is our response, with no muss or fuss.
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
        
        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<string> GetSchedule(string stopIds)
        {
            string[] pieces = stopIds.Split(',');
            
            var todaySchedule = await TransitClient.GetSchedule(pieces);
            return JsonConvert.SerializeObject(todaySchedule);
        }

        /// <summary>
        /// Initiates a data import from Google Transit.
        /// </summary>
        [HttpGet]
        [Route("tasks/google")]
        public string DoGoogleTask()
        {
            var googleRoutes = GoogleTransitImport.GoogleRoutes.Value;
            if (googleRoutes.Item1.Any())
            {
                StorageManager.UpdateRoutes(googleRoutes.Item1);
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
            var stops = TransitClient.CreateStops();
            StorageManager.Put(stops);

            var routes = TransitClient.CreateRoutes();
            StorageManager.Put(routes);

            var platformTags = TransitClient.CreatePlatformTags();
            StorageManager.Put(platformTags);

            var schedule = TransitClient.CreateSchedule();
            StorageManager.Put(schedule);

            CacheManager.ClearCache();

            return "Init job successful.";
        }
    }
}
