using CorvallisBus.Core.WebClients;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CorvallisBus.Test
{
    public class TransitClientTests
    {
        [Fact]
        public void ValidateInitJob()
        {
            var client = new TransitClient();
            _ = client.LoadTransitData();
        }
    }
}
