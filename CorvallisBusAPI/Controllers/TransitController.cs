using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.OptionsModel;
using API.DataAccess;
using API.WebClients;
using System;
using API.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

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
            try
            {
                var staticDataJson = await _repository.GetStaticDataAsync();
                return Content(staticDataJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
        }

        public List<int> ParseStopIds(string stopIds)
        {
            if (string.IsNullOrWhiteSpace(stopIds))
            {
                return new List<int>();
            }

            // ToList() this to force any parsing exception to happen here,
            // rather than later, because I'm lazy and don't wanna reason my way
            // through deferred execution and exception-handling.
            return stopIds.Split(',').Select(id => int.Parse(id)).ToList();
        }

        /// <summary>
        /// As the name suggests, this gets the ETA information for any number of stop IDs.  The data
        /// is represented as a dictionary, where the keys are the given stop IDs and the values are dictionaries.
        /// These nested dictionaries have route numbers as the keys and integers (ETA) as the values.
        /// </summary>
        [Route("eta/{stopIds}")]
        public async Task<ActionResult> GetETAs(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return HttpBadRequest();
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return HttpBadRequest();
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, parsedStopIds);
                return Json(etas);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
        }

        /// <summary>
        /// Generates a new LatLong based on input.  Throws an exception if it can't do it.
        /// </summary>
        private static LatLong? ParseUserLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return null;
            }

            var locationPieces = location.Split(',');
            if (locationPieces.Length != 2)
            {
                throw new FormatException("2 comma-separated numbers must be provided in the location string.");
            }

            return new LatLong(double.Parse(locationPieces[0]),
                               double.Parse(locationPieces[1]));
        }

        /// <summary>
        /// Endpoint for the Corvallis Bus iOS app's favorites extension.
        /// </summary>
        [Route("favorites")]
        public async Task<ActionResult> GetFavoritesViewModel(string location, string stops)
        {
            LatLong? userLocation;
            List<int> parsedStopIds;

            try
            {
                userLocation = ParseUserLocation(location);
                parsedStopIds = ParseStopIds(stops);
            }
            catch (FormatException)
            {
                return HttpBadRequest();
            }

            if (userLocation == null && (parsedStopIds == null || parsedStopIds.Count == 0))
            {
                return HttpBadRequest();
            }
            
            try
            {
                var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _getCurrentTime(), parsedStopIds, userLocation);
                return Json(viewModel);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
        }

        /// <summary>
        /// Exposes the schedule that CTS routes adhere to for a set of stops.
        /// </summary>
        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<ActionResult> GetSchedule(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return HttpBadRequest();
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return HttpBadRequest();
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _getCurrentTime(), parsedStopIds);
                return Json(todaySchedule);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
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
