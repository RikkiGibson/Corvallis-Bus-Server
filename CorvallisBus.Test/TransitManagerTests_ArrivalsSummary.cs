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

    public partial class TransitManagerTests
    {
        [Fact]
        public void TestImminentScheduledTimesIndicateRunningLate()
        {
            DateTimeOffset testTime = new DateTime(2015, 10, 3, 12, 00, 00);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>> {
                [12345] = new List<BusStopRouteSchedule> {
                    new BusStopRouteSchedule(
                        RouteNo: "TEST",
                        DaySchedules: new List<BusStopRouteDaySchedule> {
                            new BusStopRouteDaySchedule(
                                Days: DaysOfWeek.All,
                                Times: new List<TimeSpan> {
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
                        Routes: new Dictionary<string, BusRoute> {
                            ["TEST"] = new BusRoute("TEST", new List<int> { 12345 }, "", "", "")
                        },
                        Stops: new Dictionary<int, BusStop> {
                            [12345] = new BusStop(0, "", 0, 0, 0, RouteNames: new List<string> { "TEST" })
                        }
                    )));
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                PlatformTag: 123,
                RouteEstimatedArrivals: new List<ConnexionzRouteET> {
                    new ConnexionzRouteET(
                        RouteNo: "TEST",
                        EstimatedArrivalTime: new List<int>()
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expected = new List<RouteArrivalsSummary>
            {
                new RouteArrivalsSummary(
                    RouteName: "TEST",
                    ArrivalsSummary: "Over 30 minutes, then 1:25 PM",
                    ScheduleSummary: "Hourly until 3:25 PM"
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
                            RouteNo: "TEST1",
                            DaySchedules: new List<BusStopRouteDaySchedule> {
                                new BusStopRouteDaySchedule(
                                    Days: DaysOfWeek.All,
                                    Times: new List<TimeSpan> {
                                        new TimeSpan(12, 24, 0),
                                        new TimeSpan(13, 24, 0),
                                        new TimeSpan(14, 24, 0),
                                        new TimeSpan(15, 24, 0),
                                    }
                                )
                            }
                        ),
                        new BusStopRouteSchedule(
                            RouteNo: "TEST2",
                            DaySchedules: new List<BusStopRouteDaySchedule> {
                                new BusStopRouteDaySchedule(
                                    Days: DaysOfWeek.All,
                                    Times: new List<TimeSpan> {
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
                        Routes: new Dictionary<string, BusRoute> {
                            ["TEST1"] = new BusRoute("TEST1", new List<int> { 12345 }, Color: "", Url: "", Polyline: ""),
                            ["TEST2"] = new BusRoute("TEST2", new List<int> { 12345 }, Color: "", Url: "", Polyline: "")
                        },
                        Stops: new Dictionary<int, BusStop> {
                            [12345] = new BusStop(0, "", 0, 0, 0, RouteNames: new List<string> { "TEST1", "TEST2" })
                        }
                    )));

            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(
                Task.FromResult(new Dictionary<int, int> { [12345] = 123 }));

            var testEstimate = new ConnexionzPlatformET(
                PlatformTag: 123,
                RouteEstimatedArrivals: new List<ConnexionzRouteET> {
                    new ConnexionzRouteET(
                        RouteNo: "TEST1",
                        EstimatedArrivalTime: new List<int> { }
                    ),
                    new ConnexionzRouteET(
                        RouteNo: "TEST2",
                        EstimatedArrivalTime: new List<int> { 25 }
                    )
                }
            );

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expected = new List<RouteArrivalsSummary> {
                new RouteArrivalsSummary(
                    RouteName: "TEST2",
                    ArrivalsSummary: "25 minutes, then 1:25 PM",
                    ScheduleSummary: "Hourly until 3:25 PM"
                ),
                new RouteArrivalsSummary(
                    RouteName: "TEST1",
                    ArrivalsSummary: "Over 30 minutes, then 1:24 PM",
                    ScheduleSummary: "Hourly until 3:24 PM"
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

        [Theory]
        [InlineData(30, 30)]
        [InlineData(30, 10)]
        [InlineData(29, 50)]
        public void TestScheduledTimesNoRoundingErrors(int minute, int second)
        {
            // Tuesday
            DateTimeOffset testTime = new DateTime(year: 2015, month: 10, day: 6, hour: 7, minute, second);

            var testSchedule = new Dictionary<int, IEnumerable<BusStopRouteSchedule>>
            {
                [12345] = new List<BusStopRouteSchedule>
                {
                    new BusStopRouteSchedule(
                        RouteNo: "TEST",
                        DaySchedules: new List<BusStopRouteDaySchedule>
                        {
                            new BusStopRouteDaySchedule(
                                Days: DaysOfWeek.Weekdays,
                                Times: new List<TimeSpan>
                                {
                                    new TimeSpan(hours: 10, minutes: 30, seconds: 15)
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
                        Routes: new Dictionary<string, BusRoute> {
                            ["TEST"] = new BusRoute("TEST", new List<int> { 12345 }, Color: "", Url: "", Polyline: "")
                        },
                        Stops: new Dictionary<int, BusStop> {
                            [12345] = new BusStop(0, "", 0, 0, 0, RouteNames: new List<string> { "TEST" })
                        }
                    )));
            mockRepo.Setup(repo => repo.GetScheduleAsync()).Returns(Task.FromResult(testSchedule));
            mockRepo.Setup(repo => repo.GetPlatformTagsAsync()).Returns(Task.FromResult(new Dictionary<int, int> { { 12345, 123 } }));

            var testEstimate = new ConnexionzPlatformET(
                PlatformTag: 123,
                RouteEstimatedArrivals: new List<ConnexionzRouteET>());

            var mockClient = new Mock<ITransitClient>();
            mockClient.Setup(client => client.GetEta(123)).Returns(Task.FromResult<ConnexionzPlatformET?>(testEstimate));

            var expected = new List<RouteArrivalsSummary> {
                new RouteArrivalsSummary(
                    RouteName: "TEST",
                    ArrivalsSummary: "10:30 AM",
                    ScheduleSummary: ""
                )
            };

            var actual = TransitManager.GetArrivalsSummary(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;
            Assert.Equal(expected, actual[12345]);
        }
    }
}
