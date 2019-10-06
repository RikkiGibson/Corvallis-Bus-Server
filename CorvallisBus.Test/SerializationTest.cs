using CorvallisBus.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace CorvallisBus.Test
{
    public class SerializationTest
    {
        [Fact]
        public void SerializeServerBusSchedule()
        {
            var schedule = new ServerBusSchedule
            {
                [12345] = new List<BusStopRouteSchedule>
                {
                    new BusStopRouteSchedule("TEST", new List<BusStopRouteDaySchedule>())
                }
            };

            var expected = $@"{{""12345"":[{{""RouteNo"":""TEST"",""DaySchedules"":[]}}]}}";
            var actual = JsonConvert.SerializeObject(schedule);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DeserializeServerBusSchedule()
        {
            var scheduleString = $@"{{""12345"":[{{""RouteNo"":""TEST"",""DaySchedules"":[]}}]}}";
            var schedule = JsonConvert.DeserializeObject<ServerBusSchedule>(scheduleString);

            Assert.True(schedule.Contains(12345));

            var stopSchedules = schedule[12345];
            Assert.Single(stopSchedules);

            var stopSchedule = stopSchedules[0];
            Assert.Equal("TEST", stopSchedule.RouteNo);
            Assert.Empty(stopSchedule.DaySchedules);
        }
    }
}
