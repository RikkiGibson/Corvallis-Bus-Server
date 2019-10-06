using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.WebClients;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CorvallisBus.Test
{
    public class TransitClientTests
    {
        private readonly ITestOutputHelper _output;

        public TransitClientTests(ITestOutputHelper output)
        {
            _output = output;
        }

        public sealed class IntegrationTest : FactAttribute
        {
            public IntegrationTest()
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS")))
                {
                    Skip = "Test should not run in CI";
                }
            }
        }


        [IntegrationTest]
        public async Task ValidateInitJob()
        {
            var client = new TransitClient();
            var (data, errors) = client.LoadTransitData();
            foreach (var error in errors)
            {
                _output.WriteLine(error);
            }

            var tempPath = Path.GetTempPath();
            var testPath = Path.Combine(tempPath, Path.GetRandomFileName());
            try
            {
                var tempDir = Directory.CreateDirectory(testPath);
                var writeRepo = new MemoryTransitRepository(tempDir.FullName);

                writeRepo.SetPlatformTags(data.PlatformIdToPlatformTag);
                writeRepo.SetStaticData(data.StaticData);
                writeRepo.SetSchedule(data.Schedule);

                // this ends up just reading from the same static fields. perhaps they should become instance fields.
                var readRepo = new MemoryTransitRepository(tempDir.FullName);

                Assert.Equal(data.PlatformIdToPlatformTag, await readRepo.GetPlatformTagsAsync());

                // Perhaps when these data types all get good equality methods we can simply Assert.Equal
                var actualData = await readRepo.GetStaticDataAsync();
                var expectedData = data.StaticData;
                Assert.Equal(expectedData.Routes.Count, actualData.Routes.Count);
                Assert.Equal(expectedData.Stops.Count, actualData.Stops.Count);

                Assert.Equal(((IDictionary<int, List<BusStopRouteSchedule>>)data.Schedule).Count(), ((IDictionary<int, List<BusStopRouteSchedule>>)await readRepo.GetScheduleAsync()).Count());
            }
            finally
            {
                Directory.Delete(testPath, recursive: true);
            }
        }
    }
}
