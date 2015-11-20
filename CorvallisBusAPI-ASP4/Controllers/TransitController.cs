using System.Linq;
using System.Threading.Tasks;
using API.DataAccess;
using API.WebClients;
using System;
using API.Models;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Web.Http.Cors;

namespace API.Controllers
{
    using System.Configuration;
    using System.Text;
    using System.Web.Hosting;
    using ClientBusSchedule = Dictionary<int, Dictionary<string, List<int>>>;

    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TransitController : ApiController
    {
        private ITransitRepository _repository;
        private ITransitClient _client;
        private Func<DateTimeOffset> _getCurrentTime;

        /// <summary>
        /// Dependency-injected application settings which are then passed on to other components.
        /// </summary>
        public TransitController()
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
        public HttpResponseMessage Index()
        {
            var response = Request.CreateResponse(HttpStatusCode.Found);
            response.Headers.Location = new Uri("http://github.com/RikkiGibson/Corvallis-Bus-Server");
            return response;
        }

        [HttpGet]
        [Route("static")]
        public async Task<HttpResponseMessage> GetStaticData()
        {
            try
            {
                var staticDataJson = await _repository.GetStaticDataAsync();
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(staticDataJson, Encoding.UTF8, "application/json");
                return response;
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
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
        public async Task<ClientBusSchedule> GetETAs(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            try
            {
                var etas = await TransitManager.GetEtas(_repository, _client, parsedStopIds);
                return etas;
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
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
        public async Task<List<FavoriteStopViewModel>> GetFavoritesViewModel(string location, string stops)
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
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (userLocation == null && (parsedStopIds == null || parsedStopIds.Count == 0))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            try
            {
                var viewModel = await TransitManager.GetFavoritesViewModel(_repository, _client, _getCurrentTime(), parsedStopIds, userLocation);
                return viewModel;
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Exposes the schedule that CTS routes adhere to for a set of stops.
        /// </summary>
        [HttpGet]
        [Route("schedule/{stopIds}")]
        public async Task<ClientBusSchedule> GetSchedule(string stopIds)
        {
            List<int> parsedStopIds;

            try
            {
                parsedStopIds = ParseStopIds(stopIds);
            }
            catch (FormatException)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            if (parsedStopIds == null || parsedStopIds.Count == 0)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            try
            {
                var todaySchedule = await TransitManager.GetSchedule(_repository, _client, _getCurrentTime(), parsedStopIds);
                return todaySchedule;
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
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
