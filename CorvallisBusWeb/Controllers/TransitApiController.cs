using System.Linq;
using System.Threading.Tasks;
using API.DataAccess;
using API.WebClients;
using System;
using API.Models;
using System.Collections.Generic;
using System.Net;
using System.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    [RoutePrefix("api")]
    public class TransitApiController : Controller
    {
        private readonly ITransitRepository _repository;
        private readonly ITransitClient _client;
        private readonly Func<DateTimeOffset> _getCurrentTime;

        /// <summary>
        /// Dependency-injected application settings which are then passed on to other components.
        /// </summary>
        public TransitApiController()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["UseAzureStorage"]))
            {
                var appSettings = new AppSettings(ConfigurationManager.AppSettings);
                _repository = new AzureTransitRepository(appSettings);
            }
            else
            {
                var filePath = HostingEnvironment.MapPath("~");
                _repository = new MemoryTransitRepository(filePath);
            }
            _client = new TransitClient();

            _getCurrentTime = () => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTimeOffset.Now, "Pacific Standard Time");
        }

        /// <summary>
        /// Redirects the user to the GitHub repo where this API is documented.
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
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, _client, parsedStopIds);
                var etasJson = JsonConvert.SerializeObject(etas);
                return Content(etasJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (userLocation == null && (parsedStopIds == null || parsedStopIds.Count == 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _client, _getCurrentTime(), parsedStopIds, userLocation);
                var viewModelJson = JsonConvert.SerializeObject(viewModel);
                return Content(viewModelJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _client, _getCurrentTime(), parsedStopIds);
                var todayScheduleJson = JsonConvert.SerializeObject(todaySchedule);
                return Content(todayScheduleJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
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
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var arrivalsSummary = await TransitManager.GetArrivalsSummary(_repository, _client, _getCurrentTime(), parsedStopIds);
                var arrivalsSummaryJson = JsonConvert.SerializeObject(arrivalsSummary);
                return Content(arrivalsSummaryJson, "application/json");
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Performs a first-time setup and import of static data.
        /// </summary>
        [HttpGet]
        [Route("tasks/init")]
        public string Init()
        {
            var staticData = _client.CreateStaticData();
            _repository.SetStaticData(staticData);

            var platformTags = _client.CreatePlatformTags();
            _repository.SetPlatformTags(platformTags);

            var schedule = _client.CreateSchedule();
            _repository.SetSchedule(schedule);

            return "Init job successful.";
        }
    }
}
