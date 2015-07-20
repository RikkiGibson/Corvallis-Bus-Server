using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CorvallisTransit.Models
{
    /// <summary>
    /// A bus stop in the Corvallis Transit System. This is analogous to the Platform entity in Connexionz.
    /// </summary>
    public class BusStop : IEqualityComparer<BusStop>
    {
        /// <summary>
        /// This stop tag is used to get ETAs for the stop from Connexionz.
        /// </summary>
        public string StopTag { get; set; }

        /// <summary>
        /// The name of the stop, for example: "NW Monroe Ave & NW 7th St".
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The latitude value for the stop (between -90 and 90 degrees).
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// The longitude value for the stop (between -180 and 180 degrees).
        /// </summary>
        public double Long { get; set; }

        /// <summary>
        /// The stop ID. This is what is written on the bus stop signs in real life.
        /// </summary>
        public string Id { get; set; }

        public override bool Equals(object obj)
        {
            var stop = obj as BusStop;
            return stop != null ? Id == stop.Id : false;
        }

        // TODO: figure out how to organize scheduled arrival information for a route at a stop.

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(BusStop x, BusStop y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(BusStop obj)
        {
            return obj.GetHashCode();
        }
    }
}
