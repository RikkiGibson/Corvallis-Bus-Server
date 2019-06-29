using System;
using Xunit;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CorvallisBus.Core.Models;
using CorvallisBus.Core.DataAccess;
using CorvallisBus.Core.Models.Connexionz;
using CorvallisBus.Core.WebClients;

namespace CorvallisBus.Test
{

    public class TransitManagerTests
    {
        #region Schedule Interpolation
        [Fact]
        public void TestEstimateReplacesScheduledTime()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(13, 25, 0),
                                        new TimeSpan(14, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 20 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime>
            {
                new BusArrivalTime(20, isEstimate: true),
                new BusArrivalTime(85, isEstimate: false),
                new BusArrivalTime(145, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [Fact]
        public void TestScheduledTimesBeforeCutoffWithoutEstimates()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(13, 25, 0),
                                        new TimeSpan(14, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>()
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(25, isEstimate: false),
                new BusArrivalTime(85, isEstimate: false),
                new BusArrivalTime(145, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [Fact]
        public void TestEstimatePrependedToScheduledTimes()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 3 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(3, isEstimate: true),
                new BusArrivalTime(25, isEstimate: false),
                new BusArrivalTime(55, isEstimate: false),
                new BusArrivalTime(85, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [Fact]
        public void TestSecondEstimateReplacesScheduledTime()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 3, 28 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(3, isEstimate: true),
                new BusArrivalTime(28, isEstimate: true),
                new BusArrivalTime(55, isEstimate: false),
                new BusArrivalTime(85, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        /// <summary>
        /// At the peak time of day for route 6, I've noticed the schedule coming in out of order sometimes.
        /// I haven't been able to get on the city API to figure out what data is causing the issue.
        /// Once I'm able to create a test case, this is where it should live.
        /// For now I'm gonna try to get by just adding a sort call in the schedule endpoint.
        /// </summary>
        [Fact]
        public void TestUnsortedEstimatesHaveSortedOutput()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 20, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(12, 55, 0),
                                        new TimeSpan(13, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 28, 12, 1 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(1, isEstimate: true),
                new BusArrivalTime(12, isEstimate: true),
                new BusArrivalTime(28, isEstimate: true),
                new BusArrivalTime(55, isEstimate: false),
                new BusArrivalTime(85, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [Fact]
        public void TestArrivalTimesAfterMidnightRenderCorrectly()
        {
            // This happens to be a Sunday morning.
            DateTimeOffset testTime = new DateTime(2015, 10, 4, 1, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.NightOwl,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(23, 25, 0),
                                        new TimeSpan(24, 25, 0),
                                        new TimeSpan(25, 25, 0),
                                        new TimeSpan(26, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 28 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(28, isEstimate: true),
                new BusArrivalTime(85, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }

        [Fact]
        public void TestNightOwlTimesBeforeMidnightRenderCorrectly()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 3, 23, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                {
                    12345,
                    new List<BusStopRouteSchedule>
                    {
                        new BusStopRouteSchedule(
                            routeNo: "TEST",
                            daySchedules: new List<BusStopRouteDaySchedule>
                            {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.NightOwl,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(23, 25, 0),
                                        new TimeSpan(24, 25, 0),
                                        new TimeSpan(25, 25, 0),
                                        new TimeSpan(26, 25, 0),
                                    }
                                )
                            }
                        )
                    }
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET>
                {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int> { 28 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expectedArrivalTimes = new List<BusArrivalTime> {
                new BusArrivalTime(28, isEstimate: true),
                new BusArrivalTime(85, isEstimate: false),
                new BusArrivalTime(145, isEstimate: false),
                new BusArrivalTime(205, isEstimate: false)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.True(Enumerable.SequenceEqual(expectedArrivalTimes, actual[12345]["TEST"]));
        }
        #endregion

        #region Arrivals Summaries
        [Fact]
        public void TestImminentScheduledTimesIndicateRunningLate()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 3, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>> {
                [12345] = new List<BusStopRouteSchedule> {
                    new BusStopRouteSchedule(
                        routeNo: "TEST",
                        daySchedules: new List<BusStopRouteDaySchedule> {
                            new BusStopRouteDaySchedule(
                                days: DaysOfWeek.All,
                                times: new List<TimeSpan> {
                                    new TimeSpan(12, 25, 0),
                                    new TimeSpan(13, 25, 0),
                                    new TimeSpan(14, 25, 0),
                                    new TimeSpan(15, 25, 0),
                                }
                            )
                        }
                    )
                }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetStaticDataAsync()).Returns(
                Task.FromResult(
                    new BusStaticData(
                        routes: new Dictionary<string, BusRoute> {
                            ["TEST"] = new BusRoute("TEST", new List<int> { 12345 }, "", "", "")
                        },
                        stops: new Dictionary<int, BusStop> {
                            [12345] = new BusStop(0, "", 0, 0, 0, routeNames: new List<string> { "TEST" })
                        }
                    )));
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET> {
                    new ConnexionzRouteET(
                        routeNo: "TEST",
                        estimatedArrivalTime: new List<int>()
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expected = new List<RouteArrivalsSummary>
            {
                new RouteArrivalsSummary(
                    routeName: "TEST",
                    arrivalsSummary: "Over 30 minutes, then 1:25 PM",
                    scheduleSummary: "Hourly until 3:25 PM"
                )
            };

            var actual = TransitManager.GetArrivalsSummary(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.Equal(expected.Count, actual[12345].Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].RouteName, actual[12345][i].RouteName);
                Assert.Equal(expected[i].ArrivalsSummary, actual[12345][i].ArrivalsSummary);
                Assert.Equal(expected[i].ScheduleSummary, actual[12345][i].ScheduleSummary);
            }
        }

        [Fact]
        public void TestLateScheduledTimesOrderedAfterEstimates()
        {
            DateTimeOffset testTime = new DateTime(year: 2015, month: 10, day: 3, hour: 12, minute: 00, second: 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>> {
                [12345] = new List<BusStopRouteSchedule> {
                        new BusStopRouteSchedule(
                            routeNo: "TEST1",
                            daySchedules: new List<BusStopRouteDaySchedule> {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan> {
                                        new TimeSpan(12, 24, 0),
                                        new TimeSpan(13, 24, 0),
                                        new TimeSpan(14, 24, 0),
                                        new TimeSpan(15, 24, 0),
                                    }
                                )
                            }
                        ),
                        new BusStopRouteSchedule(
                            routeNo: "TEST2",
                            daySchedules: new List<BusStopRouteDaySchedule> {
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.All,
                                    times: new List<TimeSpan> {
                                        new TimeSpan(12, 25, 0),
                                        new TimeSpan(13, 25, 0),
                                        new TimeSpan(14, 25, 0),
                                        new TimeSpan(15, 25, 0),
                                    }
                                )
                            }
                        )
                    }
            };

            var mockRepo = new Mock<ITransitRepository>();
            mockRepo.Setup(repo => repo.GetStaticDataAsync()).Returns(
                Task.FromResult(
                    new BusStaticData(
                        routes: new Dictionary<string, BusRoute>(),
                        stops: new Dictionary<int, BusStop> {
                            [12345] = new BusStop(0, "", 0, 0, 0, routeNames: new List<string> { "TEST1", "TEST2" })
                        }
                    )));

            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(
                Task.FromResult(new Dictionary<int, int> { [12345] = 123 }));

            var testEstimate = new ConnexionzPlatformET(
                platformTag: 123,
                routeEstimatedArrivals: new List<ConnexionzRouteET> {
                    new ConnexionzRouteET(
                        routeNo: "TEST1",
                        estimatedArrivalTime: new List<int> { }
                    ),
                    new ConnexionzRouteET(
                        routeNo: "TEST2",
                        estimatedArrivalTime: new List<int> { 25 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expected = new List<RouteArrivalsSummary> {
                new RouteArrivalsSummary(
                    routeName: "TEST2",
                    arrivalsSummary: "25 minutes, then 1:25 PM",
                    scheduleSummary: "Hourly until 3:25 PM"
                ),
                new RouteArrivalsSummary(
                    routeName: "TEST1",
                    arrivalsSummary: "Over 30 minutes, then 1:24 PM",
                    scheduleSummary: "Hourly until 3:24 PM"
                )
            };

            var actual = TransitManager.GetArrivalsSummary(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.Equal(expected.Count, actual[12345].Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].RouteName, actual[12345][i].RouteName);
                Assert.Equal(expected[i].ArrivalsSummary, actual[12345][i].ArrivalsSummary);
                Assert.Equal(expected[i].ScheduleSummary, actual[12345][i].ScheduleSummary);
            }
        }
        #endregion
    }
}
