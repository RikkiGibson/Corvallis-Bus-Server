using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using API;
using Moq;
using API.DataAccess;
using System.Collections.Generic;
using API.Models;
using System.Threading.Tasks;

namespace CorvallisBusTests
{
    [TestClass]
    public class TransitManagerTests
    {

        // Schedule: 25, 85, 145, ...

        // Estimate: 28 Schedule: 25, 85, 145, ... Output: 28, 85, 145, ...

        // Estimate: 3 Schedule: 60, 120, 180, ... Output: 3, 60, 120, 180, ...

        // Estimate: 12 Schedule: 18, 78, 138, ... Output: 12, 78, 138

        // Estimate: 20 Schedule: 5, 65, 125, ... Output: 20, 65, 125, ...

        // Estimate: [] Schedule: 5, 65, 125, ... Output: 65, 125, ...

        [TestMethod]
        public void TestEstimateReplacesScheduledTime()
        {
            var mockRepo = new Mock<ITransitRepository>();

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 40, 0)
                                    }
                                }
                            }
                        }
                    }
                }
            };

            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            // TODO: make transitclient and therefore transitmanager instance-based so that GetEta can be mocked

            Func<DateTime> getCurrentTime = () => new DateTime(2015, 10, 20, 12, 00, 00);
            
        }
    }
}
