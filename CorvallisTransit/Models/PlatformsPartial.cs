using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CorvallisTransit.Models
{
    public partial class Platforms
    {
        public IEnumerable<PlatformsPlatform> Stops
        {
            get
            {
                var plats = Items.Skip(1).Select(i => i as PlatformsPlatform);
                return plats;
            }
        }

        public PlatformsContent ContentInfo
        {
            get { return Items.First() as PlatformsContent; }
        }
    }
}