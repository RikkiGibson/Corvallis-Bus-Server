﻿using API.Models;
using API.Models.GoogleTransit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Components
{
    /// <summary>
    /// Container for functionality which handles Blob storage in Azure.
    /// </summary>
    public static class StorageManager
    {
        public const string ROUTES_KEY = "routes";
        public const string STOPS_KEY = "stops";
        public const string PLATFORM_TAGS_KEY = "platformTags";
        public const string SCHEDULE_KEY = "schedule";

        // Nothing about Configuration is accurate right now, so this is why I'm hard-coding this crap.
        public const string BLOB_STORAGE_CONN_STRING = "DefaultEndpointsProtocol=https;AccountName=corvallisbus;AccountKey=vogmmb7EQaL7smA1V7jpR6530A/HQrU9PsgGABy0mVBM5fRXSbhphvGm456az7L+mYWwvyJ7suD3sudfM8MCiQ==";
        private const string BLOB_CONATINER_NAME = "blobstore";

        /// <summary>
        /// Gets the JSON-encoded CTS routes from Azure.
        /// </summary>
        public static async Task<string> GetStaticRouteDataAsync()
        {
            var blob = GetBlockBlob(ROUTES_KEY);
            return await blob.DownloadTextAsync();
        }

        /// <summary>
        /// Gets the JSON-encoded CTS stops from Azure.
        /// </summary>
        public static async Task<string> GetStaticStopDataAsync()
        {
            var blob = GetBlockBlob(STOPS_KEY);
            return await blob.DownloadTextAsync();
        }

        public static async Task<string> GetPlatformTagsAsync()
        {
            var blob = GetBlockBlob(PLATFORM_TAGS_KEY);
            return await blob.DownloadTextAsync();
        }

        public static async Task<string> GetScheduleAsync()
        {
            var blob = GetBlockBlob(SCHEDULE_KEY);
            return await blob.DownloadTextAsync();
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

            var googleRoutesLookup = googleRoutes.ToDictionary(gr => gr.ConnexionzName);

            Action<BusRoute> UpdateColorsAndUrls = (r) =>
            {
                r.Color = googleRoutesLookup[r.RouteNo]?.Color;
                r.Url = googleRoutesLookup[r.RouteNo]?.Url.Replace(@"\/", "/");
            };

            routes.ForEach(UpdateColorsAndUrls);

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

        public static void Put(Dictionary<string, IEnumerable<BusStopRouteSchedule>> schedule)
        {
            if (schedule == null || !schedule.Any())
            {
                throw new ArgumentNullException(nameof(schedule), "An empty schedule can't be put in the datastore.");
            }

            CloudBlockBlob blob = GetBlockBlob(SCHEDULE_KEY);

            string json = JsonConvert.SerializeObject(schedule);

            blob.UploadText(json);
        }

        /// <summary>
        /// Given the name of a block blob, gets a reference to allow read/write to that blob.
        /// </summary>
        private static CloudBlockBlob GetBlockBlob(string blockBlobName)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(BLOB_STORAGE_CONN_STRING);

            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(BLOB_CONATINER_NAME);

            return container.GetBlockBlobReference(blockBlobName);
        }
    }
}