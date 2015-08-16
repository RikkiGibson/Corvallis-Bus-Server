using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Microsoft.Framework.OptionsModel;
using API.DataAccess;
using API.WebClients;
using System;

namespace API.Controllers
{
    [Route("[controller]")]
    public class TransitController : Controller
    {
        private ITransitRepository _repository;
        private Func<DateTimeOffset> _getCurrentTime;

        /// <summary>
        /// Dependency-injected application settings which are then passed on to other components.
        /// </summary>
        public TransitController(IOptions<AppSettings> appSettings)
        {
            _repository = new TransitRepository(appSettings.Options);
            _getCurrentTime = () => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, "Pacific Standard Time");
        }

        [HttpGet]
        [Route("static")]
        public async Task<string> GetStaticData()
        {
            var staticData = await _repository.GetStaticDataAsync();
            return staticData;
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
            var etas = await TransitManager.GetEtas(_repository, splitStopIds);
            return JsonConvert.SerializeObject(etas);
        }
        
        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<string> GetSchedule(string stopIds)
        {
            string[] pieces = stopIds.Split(',');
            
            var todaySchedule = await TransitManager.GetSchedule(_repository, _getCurrentTime, pieces);
            return JsonConvert.SerializeObject(todaySchedule);
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public string Init()
        {
            // perhaps a named type should be declared when setting the static data, but
            // deserialization/reserialization should be optional when getting it.
            var staticData = TransitClient.CreateStaticData();
            var staticDataJson = JsonConvert.SerializeObject(staticData);
            _repository.SetStaticData(staticDataJson);

            var platformTags = TransitClient.CreatePlatformTags();
            _repository.SetPlatformTags(platformTags);

            var schedule = TransitClient.CreateSchedule();
            _repository.SetSchedule(schedule);

            return "Init job successful.";
        }
    }
}
