using CorvallisBus.Core.WebClients;
using System;
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
        public void ValidateInitJob()
        {
            var client = new CorvallisTransitClient();
            var (_, errors) = client.LoadTransitData();
            foreach (var error in errors)
            {
                _output.WriteLine(error);
            }
        }

        [IntegrationTest]
        public void KingInitJob()
        {
            var client = new KingTransitClient();
            var (_, errors) = client.LoadTransitData();
            foreach (var error in errors)
            {
                _output.WriteLine(error);
            }
        }
    }
}
