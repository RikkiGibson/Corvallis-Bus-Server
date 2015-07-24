using CorvallisTransit.Models;
using CorvallisTransit.Models.GoogleTransit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CorvallisTransit.Components
{
    /// <summary>
    /// Container for functionality which handles Blob storage in Azure.
    /// </summary>
    public static class StorageManager
    {
        public const string ROUTES_KEY = "routes";
        public const string STOPS_KEY = "stops";
        public const string PLATFORM_TAGS_KEY = "platformTags";

        /// <summary>
        /// Gets the JSON-encoded CTS routes from Azure.
        /// </summary>
        public static async Task<List<BusRoute>> GetRoutesAsync()
        {
            var blob = GetBlockBlob(ROUTES_KEY);
            string json = await blob.DownloadTextAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<List<BusRoute>>(json));
        }

        /// <summary>
        /// Gets the JSON-encoded CTS stops from Azure.
        /// </summary>
        public static async Task<List<BusStop>> GetStopsAsync()
        {
            var blob = GetBlockBlob(STOPS_KEY);
            string json = await blob.DownloadTextAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<List<BusStop>>(json));
        }

        public static async Task<Dictionary<string, string>> GetPlatformTagsAsync()
        {
            var blob = GetBlockBlob(PLATFORM_TAGS_KEY);
            string json = await blob.DownloadTextAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, string>>(json));
        }

        /// <summary>
        /// Updates the routes in the routes blob with new google transit data.
        /// </summary>
        public static void UpdateRoutes(List<GoogleRoute> googleRoutes)
        {
            if (googleRoutes == null || !googleRoutes.Any())
            {
                throw new ArgumentNullException(nameof(googleRoutes), "Google routes must have a value.");
            }

            CloudBlockBlob blob = GetBlockBlob(ROUTES_KEY);

            string json = blob.DownloadText();

            List<BusRoute> routes = JsonConvert.DeserializeObject<List<BusRoute>>(json);

            // Filter out the ones that are currently inactive so that we don't garble up colors and URLs.
            googleRoutes = googleRoutes.Where(gr => routes.Any(r => r.RouteNo == gr.Name)).ToList();

            for (int i = 0; i < routes.Count; i++)
            {
                routes[i].Color = googleRoutes[i]?.Color;
                routes[i].Url = googleRoutes[i]?.Url;
            }

            json = JsonConvert.SerializeObject(routes);

            blob.UploadText(json);
        }

        /// <summary>
        /// Puts a list of CTS Routes into an Azure Blob as JSON.
        /// </summary>
        public static void Put(List<BusRoute> routes)
        {
            if (routes == null || !routes.Any())
            {
                throw new ArgumentNullException(nameof(routes), "CTS routes need some data!");
            }

            CloudBlockBlob blob = GetBlockBlob(ROUTES_KEY);

            string json = JsonConvert.SerializeObject(routes);

            blob.UploadText(json);
        }

        /// <summary>
        /// Puts a list of CTS Stops into an Azure Blob as JSON.
        /// </summary>
        public static void Put(List<BusStop> stops)
        {
            if (stops == null || !stops.Any())
            {
                throw new ArgumentNullException(nameof(stops), "CTS routes need some data!");
            }

            CloudBlockBlob blob = GetBlockBlob(STOPS_KEY);

            string json = JsonConvert.SerializeObject(stops);

            blob.UploadText(json);
        }   
        
        /// <summary>
        /// Puts a dictionary that takes a PlatformNo (5-digit number) to PlatformTag (3-digit number).
        /// </summary>
        /// <param name="platformTags"></param>
        public static void Put(Dictionary<string, string> platformTags)
        {
            if (platformTags == null || !platformTags.Any())
            {
                throw new ArgumentNullException(nameof(platformTags), "An empty dictionary can't be put in the datastore.");
            }

            CloudBlockBlob blob = GetBlockBlob(PLATFORM_TAGS_KEY);

            string json = JsonConvert.SerializeObject(platformTags);

            blob.UploadText(json);
        }



        /// <summary>
        /// Given the name of a block blob, gets a reference to allow read/write to that blob.
        /// </summary>
        private static CloudBlockBlob GetBlockBlob(string blockBlobName)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["BlobStorageConnectionString"]);

            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference("routesandstopsstore");

            return container.GetBlockBlobReference(blockBlobName);
        }
    }
}