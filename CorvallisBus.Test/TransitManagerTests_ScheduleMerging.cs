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
        public const DaysOfWeek NightOwl = DaysOfWeek.Thursday | DaysOfWeek.Friday | DaysOfWeek.Saturday;

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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Theory]
        [InlineData(DaysOfWeek.Thursday, 2)] // friday morning
        [InlineData(DaysOfWeek.Friday, 3)] // saturday morning
        [InlineData(DaysOfWeek.Saturday, 4)] // sunday morning
        [InlineData(DaysOfWeek.Sunday, 5)] // monday morning
        [InlineData(NightOwl, 2)] // friday morning
        [InlineData(NightOwl, 3)] // saturday morning
        [InlineData(NightOwl, 4)] // sunday morning
        public void TestArrivalTimesAfterMidnightRenderCorrectly(DaysOfWeek daysOfWeek, int dayOfMonth)
        {
            DateTimeOffset testTime = new DateTime(year: 2015, month: 10, dayOfMonth, hour: 1, minute: 00, second: 00);

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
                                    days: daysOfWeek,
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Theory]
        [InlineData(DaysOfWeek.Friday, 11)] // sunday
        [InlineData(NightOwl, 12)] // monday
        [InlineData(NightOwl, 7)] // monday
        public void TestArrivalTimesAfterMidnightRequireCorrectDay(DaysOfWeek daysOfWeek, int dayOfMonth)
        {
            DateTimeOffset testTime = new DateTime(year: 2015, month: 10, dayOfMonth, hour: 1, minute: 00, second: 00);

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
                                    days: daysOfWeek,
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
                new BusArrivalTime(28, isEstimate: true)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
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
                                    days: NightOwl,
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Fact]
        public void EstimatesAreForMonday_WhenEarlyMondayMorningOfWeekday()
        {
            DateTimeOffset testTime = new DateTime(2019, 9, 30, 7, 00, 00);

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
                                    days: DaysOfWeek.Weekdays,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(6, 25, 0),
                                        new TimeSpan(7, 25, 0),
                                        new TimeSpan(8, 25, 0),
                                        new TimeSpan(9, 25, 0),
                                        new TimeSpan(10, 25, 0)
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Fact]
        public void EstimatesAreForThursday_WhenEarlyThursdayMorningOfNightOwl()
        {
            DateTimeOffset testTime = new DateTime(2019, 10, 3, 7, 00, 00);

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
                                    days: NightOwl,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(6, 25, 0),
                                        new TimeSpan(7, 25, 0),
                                        new TimeSpan(8, 25, 0),
                                        new TimeSpan(9, 25, 0),
                                        new TimeSpan(10, 25, 0)
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

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Fact]
        public void EstimatesAreForPreviousDay_WhenAdjacentDays_AndEarlyMorningWithOvernightBus()
        {
            DateTimeOffset testTime = new DateTime(2019, 10, 1, 3, 28, 00);

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
                                    days: DaysOfWeek.Tuesday,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(6, 25, 0),
                                        new TimeSpan(7, 25, 0),
                                        new TimeSpan(8, 25, 0),
                                        new TimeSpan(9, 25, 0),
                                        new TimeSpan(10, 25, 0)
                                    }
                                ),
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.Monday,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(25, 25, 0),
                                        new TimeSpan(26, 25, 0),
                                        new TimeSpan(27, 25, 0),
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
                new BusArrivalTime(3, isEstimate: true)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }

        [Fact]
        public void EstimatesAreForPreviousDay_WhenAdjacentDays_AndEarlyMorningWithOvernightBus_ButPathologicalOrderingOfSchedules()
        {
            DateTimeOffset testTime = new DateTime(year: 2019, month: 10, day: 1, hour: 3, minute: 28, second: 00);

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
                                    days: DaysOfWeek.Monday,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(25, 25, 0),
                                        new TimeSpan(26, 25, 0),
                                        new TimeSpan(27, 25, 0),
                                    }
                                ),
                                new BusStopRouteDaySchedule(
                                    days: DaysOfWeek.Tuesday,
                                    times: new List<TimeSpan>
                                    {
                                        new TimeSpan(6, 25, 0),
                                        new TimeSpan(7, 25, 0),
                                        new TimeSpan(8, 25, 0),
                                        new TimeSpan(9, 25, 0),
                                        new TimeSpan(10, 25, 0)
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
                new BusArrivalTime(3, isEstimate: true)
            };
            var actual = TransitManager.GetSchedule(mockRepo.Object, mockClient.Object, testTime, new List<int> { 12345 }).Result;

            Assert.Equal(expectedArrivalTimes, actual[12345]["TEST"]);
        }
    }
}
