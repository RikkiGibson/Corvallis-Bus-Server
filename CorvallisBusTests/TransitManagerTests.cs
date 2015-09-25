using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using API;
using Moq;
using API.DataAccess;
using System.Collections.Generic;
using API.Models;
using System.Threading.Tasks;
using API.WebClients;
using API.Models.Connexionz;
using System.Linq;

namespace CorvallisBusTests
{
    [TestClass]
    public class TransitManagerTests
    {
        [TestMethod]
        public void TestEstimateReplacesScheduledTime()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            RouteNo = "TEST",
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(13, 25, 0),
                                        new TimeSpan(14, 25, 0),
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET
            {
                PlatformTag = 123,
                RouteEstimatedArrivals = new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET
                    {
                        RouteNo = "TEST",
                        EstimatedArrivalTime = new List<int> { 20 }
                    }
                }
            };

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult(testEstimate));

            var expectedArrivalTimes = new List<int> { 20, 85, 145 };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.IsTrue(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [TestMethod]
        public void TestScheduledTimesBeforeCutoffWithoutEstimates()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            RouteNo = "TEST",
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(13, 25, 0),
                                        new TimeSpan(14, 25, 0),
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET
            {
                PlatformTag = 123,
                RouteEstimatedArrivals = new List<ConnexionzRouteET>()
            };

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult(testEstimate));

            var expectedArrivalTimes = new List<int> { 25, 85, 145 };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.IsTrue(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [TestMethod]
        public void TestEstimatePrependedToScheduledTimes()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            RouteNo = "TEST",
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET
            {
                PlatformTag = 123,
                RouteEstimatedArrivals = new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET
                    {
                        RouteNo = "TEST",
                        EstimatedArrivalTime = new List<int> { 3 }
                    }
                }
            };

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult(testEstimate));

            var expectedArrivalTimes = new List<int> { 3, 25, 55, 85 };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.IsTrue(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [TestMethod]
        public void TestSecondEstimateReplacesScheduledTime()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            RouteNo = "TEST",
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET
            {
                PlatformTag = 123,
                RouteEstimatedArrivals = new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET
                    {
                        RouteNo = "TEST",
                        EstimatedArrivalTime = new List<int> { 3, 28 }
                    }
                }
            };

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult(testEstimate));

            var expectedArrivalTimes = new List<int> { 3, 28, 55, 85 };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.IsTrue(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        /// <summary>
        /// At the peak time of day for route 6, I've noticed the schedule coming in out of order sometimes.
        /// I haven't been able to get on the city API to figure out what data is causing the issue.
        /// Once I'm able to create a test case, this is where it should live.
        /// For now I'm gonna try to get by just adding a sort call in the schedule endpoint.
        /// </summary>
        [TestMethod]
        public void TestUnsortedEstimatesHaveSortedOutput()
        {
            Assert.Inconclusive();

            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule
                        {
                            RouteNo = "TEST",
                            DaySchedules = new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule
                                {
                                    Days = DaysOfWeek.All,
                                    Times = new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET
            {
                PlatformTag = 123,
                RouteEstimatedArrivals = new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET
                    {
                        RouteNo = "TEST",
                        EstimatedArrivalTime = new List<int> { 28, 1 }
                    }
                }
            };

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult(testEstimate));

            var expectedArrivalTimes = new List<int> { 1, 28, 55, 85 };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.IsTrue(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }
    }
}
