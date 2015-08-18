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
            try
            {
                var staticDataJson = await _repository.GetStaticDataAsync();
                return Content(staticDataJson, "application/json");
            }
            catch (Exception) // gotta catch 'em all
            {
                return new HttpStatusCodeResult(503);
            }
        }

        public List<int> ParseStopIds(string stopIds)
        {
            if (stopIds == null)
            {
                throw new ArgumentNullException(nameof(stopIds));
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
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                return HttpBadRequest();
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, parsedStopIds);
                return Json(etas);
            }
            catch (Exception) // gotta catch 'em all
            {
                return new HttpStatusCodeResult(503);
            }
        }

        /// <summary>
        /// Endpoint for the Corvallis Bus iOS app's favorites extension.
        /// </summary>
        [Route("favorites")]
        public async Task<ActionResult> GetFavoritesViewModel(string location, string stops)
        {
            LatLong? userLocation;
            List<int> parsedStops;

            try
            {
                parsedStops = ParseStopIds(stops);
                userLocation = ParseUserLocation(location);
            }
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                return HttpBadRequest();
            }

            // If it was somehow able to parse but couldn't get anything, they get an empty array.
            //
            // ...
            //
            // You know, kinda like how JavaScript works.
            if (userLocation == null && parsedStops == null)
            {
                return Content("[]", "application/json");
            }

            try
            {
                var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _getCurrentTime, parsedStops, userLocation, fallbackToGrayColor: false);
                return Json(viewModel);
            }
            catch (Exception) // gotta catch 'em all
            {
                return new HttpStatusCodeResult(503);
            }
        }

        /// <summary>
        /// Generates a new LatLong based on input.  Throws an exception if it can't do it.
        /// </summary>
        private static LatLong? ParseUserLocation(string location)
        {
            var locationPieces = location?.Split(',');

            return new LatLong(double.Parse(locationPieces[0]),
                               double.Parse(locationPieces[1]));
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
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                return HttpBadRequest();
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _getCurrentTime, parsedStopIds);
                return Json(todaySchedule);
            }
            catch (Exception) // gotta catch 'em all
            {
                return new HttpStatusCodeResult(503);
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
