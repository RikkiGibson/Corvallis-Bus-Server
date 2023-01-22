using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace CorvallisBus.Core.Models
{
    /// <summary>
    /// Represents the days of the week in during which a schedule is in effect.
    /// Differs from the built in DayOfWeek because it's intended to be ORed together.
    /// </summary>
    [Flags]
    public enum DaysOfWeek
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64,

        None = 0,
        All = Weekdays | Weekend,
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Sunday | Saturday
    }

    public static class DaysOfWeekUtils
    {
        public static DaysOfWeek ToDaysOfWeek(string day)
        {
            switch (day)
            {
                case "Mon": return DaysOfWeek.Monday;
                case "Tue": return DaysOfWeek.Tuesday;
                case "Wed": return DaysOfWeek.Wednesday;
                case "Thu": return DaysOfWeek.Thursday;
                case "Fri": return DaysOfWeek.Friday;
                case "Sat": return DaysOfWeek.Saturday;
                case "Sun": return DaysOfWeek.Sunday;
                default: return DaysOfWeek.None;
            }
        }

        /// <summary>
        /// There is a real reason to have this enum instead of just DayOfWeek--
        /// it's useful to be able to OR days together the way we do.
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static DaysOfWeek ToDaysOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Monday: return DaysOfWeek.Monday;
                case DayOfWeek.Tuesday: return DaysOfWeek.Tuesday;
                case DayOfWeek.Wednesday: return DaysOfWeek.Wednesday;
                case DayOfWeek.Thursday: return DaysOfWeek.Thursday;
                case DayOfWeek.Friday: return DaysOfWeek.Friday;
                case DayOfWeek.Saturday: return DaysOfWeek.Saturday;
                case DayOfWeek.Sunday: return DaysOfWeek.Sunday;
                default: return DaysOfWeek.None;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the provided DateTimeOffset falls in to the DaysOfWeek specified
        /// </summary>
        public static bool TodayMayFallInsideDaySchedule(BusStopRouteDaySchedule ds, DateTimeOffset currentTime)
        {
            DaysOfWeek currentDay = ToDaysOfWeek(currentTime.DayOfWeek);

            // simple case where current time is definitely inside the current schedule
            return (ds.Days & currentDay) == currentDay || TimeInSpilloverWindow(ds, currentTime);
        }

        /// <summary>
        /// Returns true if the current time is in the early morning of e.g. Tuesday, and this day schedule contains 'late night runs' for a Monday schedule that spill over in to Tuesday morning
        /// </summary>
        public static bool TimeInSpilloverWindow(BusStopRouteDaySchedule ds, DateTimeOffset currentTime)
        {
            DaysOfWeek previousDay = ToDaysOfWeek(currentTime.AddDays(-1).DayOfWeek);
            TimeSpan lastTime = ds.Times.Last();

            if ((ds.Days & previousDay) == previousDay && lastTime.Days >= 1)
            {
                DateTimeOffset lastScheduleDateTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, lastTime.Hours, lastTime.Minutes, 0, 0, currentTime.Offset);
                return currentTime < lastScheduleDateTime.AddMinutes(TransitManager.ESTIMATES_MAX_ADVANCE_MINUTES);
            }

            return false;
        }
    }
}