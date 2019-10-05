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
                        routes: new Dictionary<string, BusRoute> {
                            ["TEST1"] = new BusRoute("TEST1", new List<int> { 12345 }, color: "", url: "", polyline: ""),
                            ["TEST2"] = new BusRoute("TEST2", new List<int> { 12345 }, color: "", url: "", polyline: "")
                        },
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
    }
}
