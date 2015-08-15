using API.Models;
using API.Models.GoogleTransit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess
{
    /// <summary>
    /// Container for functionality which handles Blob storage in Azure.
    /// </summary>
    public class StorageManager
    {
        private string _connectionString;
        private string _containerName;
        private string _routesKey;
        private string _stopsKey;
        private string _platformTagsKey;
        private string _scheduleKey;

        public StorageManager(AppSettings appSettings)
        {
            _connectionString = appSettings.BlobStorageConnectionString;
            _containerName = appSettings.BlobContainerName;
            _routesKey = appSettings.RoutesKey;
            _stopsKey = appSettings.StopsKey;
            _platformTagsKey = appSettings.PlatformsKey;
            _scheduleKey = appSettings.SchedulesKey;
        }

        /// <summary>
        /// Gets the JSON-encoded CTS routes from Azure.
        /// </summary>
        public async Task<string> GetRoutesAsync()
        {
            var blob = GetBlockBlob(_routesKey);
            return await blob.DownloadTextAsync();
        }

        /// <summary>
        /// Gets the JSON-encoded CTS stops from Azure.
        /// </summary>
        public async Task<string> GetStopsAsync()
        {
            var blob = GetBlockBlob(_stopsKey);
            return await blob.DownloadTextAsync();
        }

        public async Task<string> GetPlatformTagsAsync()
        {
            var blob = GetBlockBlob(_platformTagsKey);
            return await blob.DownloadTextAsync();
        }

        public async Task<string> GetScheduleAsync()
        {
            var blob = GetBlockBlob(_scheduleKey);
            return await blob.DownloadTextAsync();
        }

        /// <summary>
        /// Puts a list of CTS Routes into an Azure Blob as JSON.
        /// </summary>
        public void SetRoutes(string routesJson)
        {
            if (string.IsNullOrWhiteSpace(routesJson))
            {
                throw new ArgumentNullException(nameof(routesJson), "CTS routes need some data!");
            }

            CloudBlockBlob blob = GetBlockBlob(_routesKey);
            blob.UploadText(routesJson);
        }

        /// <summary>
        /// Puts a list of CTS Stops into an Azure Blob as JSON.
        /// </summary>
        public void SetStops(string stopsJson)
        {
            if (string.IsNullOrWhiteSpace(stopsJson))
            {
                throw new ArgumentNullException(nameof(stopsJson), "CTS routes need some data!");
            }

            CloudBlockBlob blob = GetBlockBlob(_stopsKey);
            blob.UploadText(stopsJson);
        }

        /// <summary>
        /// Puts a dictionary that takes a PlatformNo (5-digit number) to PlatformTag (3-digit number).
        /// </summary>
        public void SetPlatformTags(string platformTagsJson)
        {
            if (string.IsNullOrWhiteSpace(platformTagsJson))
            {
                throw new ArgumentNullException(nameof(platformTagsJson), "An empty dictionary can't be put in the datastore.");
            }

            CloudBlockBlob blob = GetBlockBlob(_platformTagsKey);
            blob.UploadText(platformTagsJson);
        }

        public void SetSchedule(string scheduleJson)
        {
            if (string.IsNullOrWhiteSpace(scheduleJson))
            {
                throw new ArgumentNullException(nameof(scheduleJson), "An empty schedule can't be put in the datastore.");
            }

            CloudBlockBlob blob = GetBlockBlob(_scheduleKey);
            blob.UploadText(scheduleJson);
        }

        /// <summary>
        /// Given the name of a block blob, gets a reference to allow read/write to that blob.
        /// </summary>
        private CloudBlockBlob GetBlockBlob(string blockBlobName)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(_connectionString);

            CloudBlobClient client = account.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(_containerName);

            return container.GetBlockBlobReference(blockBlobName);
        }
    }
}