using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.WebClients;
using CorvallisBus.Core.Models;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;

namespace CorvallisBus.Controllers
{
    [Route("api")]
    public class TransitApiController : Controller
    {
        private static string _destinationTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "Pacific Standard Time"
                : "America/Los_Angeles";

        private readonly ITransitRepository _repository;
        private readonly ITransitClient _client;
        private readonly Func<DateTimeOffset> _getCurrentTime;

        public TransitApiController(IHostingEnvironment env)
        {
            _repository = new MemoryTransitRepository(env.WebRootPath);
            _client = new TransitClient();
            _getCurrentTime = () => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, _destinationTimeZoneId);
        }

        /// <summary>
        /// Redirects the user to the GitHub repo where this API is documented.
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            return RedirectPermanent("http://github.com/RikkiGibson/Corvallis-Bus-Server");
        }

        [HttpGet("static")]
        public async Task<ActionResult> GetStaticData()
        {
            try
            {
                var staticDataJson = await _repository.GetSerializedStaticDataAsync();
                return Content(staticDataJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
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
        [HttpGet("eta/{stopIds}")]
        public async Task<ActionResult> GetETAs(string stopIds)
        {
            List<int> parsedStopIds;
            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return StatusCode(400);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, _client, parsedStopIds);
                var etasJson = JsonConvert.SerializeObject(etas);
                return Content(etasJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
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
        [HttpGet("favorites")]
        public async Task<ActionResult> GetFavoritesViewModel(string location, string stops)
        {
            LatLong? userLocation;
            List<int> parsedStopIds;

            userLocation = ParseUserLocation(location);
            parsedStopIds = ParseStopIds(stops);

            if (userLocation == null && (parsedStopIds == null || parsedStopIds.Count == 0))
            {
                throw new ArgumentException($"One of {nameof(location)} or {nameof(stops)} must be non-empty.");
            }

            var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _client, _getCurrentTime(), parsedStopIds, userLocation);
            var viewModelJson = JsonConvert.SerializeObject(viewModel);
            return Content(viewModelJson, "application/json");
        }

        /// <summary>
        /// Exposes the schedule that CTS routes adhere to for a set of stops.
        /// </summary>
        [HttpGet("schedule/{stopIds}")]
        public async Task<ActionResult> GetSchedule(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return StatusCode(400);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _client, _getCurrentTime(), parsedStopIds);
                var todayScheduleJson = JsonConvert.SerializeObject(todaySchedule);
                return Content(todayScheduleJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("arrivals-summary/{stopIds}")]
        public async Task<ActionResult> GetArrivalsSummary(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                return StatusCode(400);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return StatusCode(400);
            }

            try
            {
                var arrivalsSummary = await TransitManager.GetArrivalsSummary(_repository, _client, _getCurrentTime(), parsedStopIds);
                var arrivalsSummaryJson = JsonConvert.SerializeObject(arrivalsSummary);
                return Content(arrivalsSummaryJson, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet("tasks/init")]
        public string Init()
        {
            var busSystemData = _client.LoadTransitData();

            _repository.SetStaticData(busSystemData.StaticData);
            _repository.SetPlatformTags(busSystemData.PlatformIdToPlatformTag);
            _repository.SetSchedule(busSystemData.Schedule);

            return "Init job successful.";
        }
    }
}
