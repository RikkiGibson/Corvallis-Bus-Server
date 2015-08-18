using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using Microsoft.Framework.OptionsModel;
using API.DataAccess;
using API.WebClients;
using System;
using API.Models;
using System.Collections.Generic;

namespace API.Controllers
{
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
        public async Task<ActionResult> GetStaticData()
        {
            var staticDataJson = await _repository.GetStaticDataAsync();
            return Content(staticDataJson, "application/json");
        }

        public IEnumerable<int> ParseStopIds(string stopIds)
        {
            if (stopIds == null)
            {
                yield break;
            }

            var splitStops = stopIds.Split(',');
            
            int parseResult;
            foreach(var stop in splitStops)
            {
                if (int.TryParse(stop, out parseResult))
                {
                    yield return parseResult;
                }
            }
        }

        /// <summary>
        /// As the name suggests, this gets the ETA information for any number of stop IDs.  The data
        /// is represented as a dictionary, where the keys are the given stop IDs and the values are dictionaries.
        /// These nested dictionaries have route numbers as the keys and integers (ETA) as the values.
        /// </summary>
        [Route("eta/{stopIds}")]
        public async Task<ActionResult> GetETAs(string stopIds)
        {
            var parsedStopIds = ParseStopIds(stopIds);
            var etas = await TransitManager.GetEtas(_repository, parsedStopIds);
            return Json(etas);
        }
        
        [Route("favorites")]
        public async Task<ActionResult> GetFavoritesViewModel(string location, string stops)
        {
            LatLong? userLocation = null;

            var locationPieces = location?.Split(',');
            double lat, lon;
            if (locationPieces?.Length == 2 &&
                double.TryParse(locationPieces?[0], out lat) &&
                double.TryParse(locationPieces?[1], out lon))
            {
                userLocation = new LatLong(lat, lon);
            }

            var parsedStops = ParseStopIds(stops);

            // No point in spinning up all the repository stuff if there's nothing to look up.
            if (userLocation == null && parsedStops == null)
            {
                return Content("[]", "application/json");
            }

            // try this URL: http://localhost:48487/transit/favorites?stops=11776,10308&location=44.5645659,-123.2620435
            var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _getCurrentTime, parsedStops, userLocation, fallbackToGrayColor: false);

            return Json(viewModel);
        }

        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<ActionResult> GetSchedule(string stopIds)
        {
            var parsedStopIds = ParseStopIds(stopIds);
            var todaySchedule = await TransitManager.GetSchedule(_repository, _getCurrentTime, parsedStopIds);

            return Json(todaySchedule);
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public ActionResult Init()
        {
            var staticData = TransitClient.CreateStaticData();
            _repository.SetStaticData(staticData);

            var platformTags = TransitClient.CreatePlatformTags();
            _repository.SetPlatformTags(platformTags);

            var schedule = TransitClient.CreateSchedule();
            _repository.SetSchedule(schedule);

            return Content("Init job successful.", "text/plain");
        }
    }
}
