using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Microsoft.Framework.OptionsModel;
using API.DataAccess;
using API.WebClients;
using System;
using API.Models;
using System.Device.Location;

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
            var staticDataJson = await _repository.GetStaticDataAsync();
            return staticDataJson;
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
        
        [Route("favorites")]
        public async Task<string> GetFavoritesViewModel(string location, string stops)
        {
            GeoCoordinate userLocation = null;

            var locationPieces = location?.Split(',');
            double lat, lon;
            if (locationPieces?.Length == 2 &&
                double.TryParse(locationPieces[0], out lat) &&
                double.TryParse(locationPieces[1], out lon))
            {
                userLocation = new GeoCoordinate(lat, lon);
            }
            

            var stopPieces = stops.Split(',');

            // try this URL: http://localhost:48487/transit/favorites?stops=11776,10308&location=44.5645659,-123.2620435
            var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _getCurrentTime, stopPieces, userLocation, fallbackToGrayColor: false);

            return JsonConvert.SerializeObject(viewModel);
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
            var staticData = TransitClient.CreateStaticData();
            _repository.SetStaticData(staticData);

            var platformTags = TransitClient.CreatePlatformTags();
            _repository.SetPlatformTags(platformTags);

            var schedule = TransitClient.CreateSchedule();
            _repository.SetSchedule(schedule);

            return "Init job successful.";
        }
    }
}
