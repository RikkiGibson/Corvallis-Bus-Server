using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Microsoft.Framework.OptionsModel;
using API.DataAccess;
using API.WebClients;

namespace API.Controllers
{
    [Route("[controller]")]
    public class TransitController : Controller
    {
        private ITransitRepository _repository;

        /// <summary>
        /// Dependency-injected application settings which are then passed on to other components.
        /// </summary>
        public TransitController(IOptions<AppSettings> appSettings)
        {
            _repository = new TransitRepository(appSettings.Options);
        }

        [HttpGet]
        [Route("static")]
        public async Task<string> GetStaticData()
        {
            // TODO: ideally, there's no construction involved in our static payloads.
            // we have the JSON string in a cache or blob and that string is our response, with no muss or fuss.
            
            var staticData = await TransitManager.GetStaticData(_repository);
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
            var etas = await TransitClient.GetEtas(_repository, splitStopIds);
            return JsonConvert.SerializeObject(etas);
        }
        
        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<string> GetSchedule(string stopIds)
        {
            string[] pieces = stopIds.Split(',');
            
            var todaySchedule = await TransitManager.GetSchedule(_repository, pieces);
            return JsonConvert.SerializeObject(todaySchedule);
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public string Init()
        {
            var stops = TransitClient.CreateStops();
            var stopsJson = JsonConvert.SerializeObject(stops);
            _repository.SetStops(stopsJson);

            var routes = TransitClient.CreateRoutes();
            var routesJson = JsonConvert.SerializeObject(routes);
            _repository.SetRoutes(routesJson);

            var platformTags = TransitClient.CreatePlatformTags();
            var platformTagsJson = JsonConvert.SerializeObject(platformTags);
            _repository.SetPlatformTags(platformTagsJson);

            var schedule = TransitClient.CreateSchedule();
            var scheduleJson = JsonConvert.SerializeObject(schedule);
            _repository.SetSchedule(scheduleJson);

            return "Init job successful.";
        }
    }
}
