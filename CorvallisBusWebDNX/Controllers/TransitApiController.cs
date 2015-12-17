using System.Linq;
using System.Threading.Tasks;
using CorvallisBusCoreNetCore.DataAccess;
using System;
using System.Collections.Generic;
using System.Net;
using CorvallisBusCoreNetCore.WebClients;
using Microsoft.AspNet.Mvc;
using CorvallisBusCoreNetCore.Models;
using CorvallisBusCoreNetCore;
using Microsoft.AspNet.Hosting;

namespace CorvallisBusWebDNX.Controllers
{
    [Route("api")]
    public class TransitApiController : Controller
    {
        private ITransitRepository _repository;
        private ITransitClient _client;
        private Func<DateTimeOffset> _getCurrentTime;

        /// <summary>
        /// Dependency-injected application settings which are then passed on to other components.
        /// </summary>
        public TransitApiController()
        {
            var filePath = new HostingEnvironment().WebRootPath;
            _repository = new MemoryTransitRepository(filePath);
            _client = new TransitClient();

            _getCurrentTime = () => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        }

        /// <summary>
        /// Redirects the user to the GitHub repo where this CorvallisBusDNX is documented.
        /// </summary>
        [HttpGet]
        [Route("")]
        public ActionResult Index()
        {
            return RedirectPermanent("http://github.com/RikkiGibson/Corvallis-Bus-Server");
        }

        [HttpGet]
        [Route("static")]
        public async Task<ActionResult> GetStaticData()
        {
            try
            {
                var staticDataJson = await _repository.GetSerializedStaticDataAsync();
                return Content(staticDataJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, _client, parsedStopIds);
                return Json(etas);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (userLocation == null && (parsedStopIds == null || parsedStopIds.Count == 0))
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            try
            {
                var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _client, _getCurrentTime(), parsedStopIds, userLocation);
                return Json(viewModel);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _client, _getCurrentTime(), parsedStopIds);
                return Json(todaySchedule);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("arrivals-summary/{stopIds}")]
        public async Task<ActionResult> GetArrivalsSummary(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            try
            {
                var arrivalsSummary = await TransitManager.GetArrivalsSummary(_repository, _client, _getCurrentTime(), parsedStopIds);
                return Json(arrivalsSummary);
            }
            catch
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public async Task<string> Init()
        {
            var staticDataTask = _client.CreateStaticDataAsync();
            var platformsTask = _client.CreatePlatformTagsAsync();
            var scheduleTask = _client.CreateScheduleAsync();

            var staticData = await staticDataTask;
            _repository.SetStaticData(staticData);

            var platformTags = await platformsTask;
            _repository.SetPlatformTags(platformTags);

            var schedule = await scheduleTask;
            _repository.SetSchedule(schedule);

            return "Init job successful.";
        }
    }
}
