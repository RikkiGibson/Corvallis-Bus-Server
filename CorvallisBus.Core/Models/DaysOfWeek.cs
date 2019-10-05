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
        Weekend = Sunday | Saturday,
        NightOwl = Thursday | Friday | Saturday, // TODO: remove this
    }

    public static class DaysOfWeekUtils
    {
        private const int MinuteBuffer = 30;
        private static readonly Regex m_dayOfWeekPattern = new Regex("Mon|Tue|Wed|Thu|Fri|Sat|Sun");

        private static DaysOfWeek ToDaysOfWeek(string day)
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
        private static DaysOfWeek ToDaysOfWeek(DayOfWeek day)
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
        /// Gets all the days of the week contained in the input string using a regular expression.
        /// </summary>
        public static DaysOfWeek GetDaysOfWeek(string days)
        {
            DaysOfWeek result = DaysOfWeek.None;
            foreach (Match match in m_dayOfWeekPattern.Matches(days))
            {
                result |= ToDaysOfWeek(match.Value);
            }
            return result;
        }

        /// <summary>
        /// Returns a value indicating whether the provided DateTimeOffset falls in to the DaysOfWeek specified
        /// </summary>
        public static bool TodayMayFallInsideDaySchedule(BusStopRouteDaySchedule ds, DateTimeOffset currentTime)
        {
            DaysOfWeek currentDay = GetDaysOfWeekFromCurrentTime(currentTime);
            DaysOfWeek previousDay = GetDaysOfWeekFromCurrentTime(currentTime.AddDays(-1));
            TimeSpan lastTime = ds.Times.Last();

            // simple case where current time is definitely inside the current schedule
            if((ds.Days & currentDay) == currentDay) {
                return true;
            }
            // slightly more annoying cass where current time is outside the *day* of current schedule, but the schedule spills into the next day 
            // and the extremely annoying case where current time is definitely outside the current schedule, but only by a bit. 
            // In this case, there might be outstanding busses from this schedule, so we'll pad it by 30 minutes or so
            else if((ds.Days & previousDay) == previousDay && lastTime.Hours >= 24) {
                DateTimeOffset lastScheduleDateTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, lastTime.Hours-24, lastTime.Minutes, 0, 0, currentTime.Offset);
                return currentTime.CompareTo(lastScheduleDateTime.AddMinutes(MinuteBuffer)) < 0;
            } else {
                return false;
            }
        }

        public static DaysOfWeek GetDaysOfWeekFromCurrentTime(DateTimeOffset currentTime) {
            switch(currentTime.DayOfWeek) {
                case DayOfWeek.Monday:
                    return DaysOfWeek.Monday;
                case DayOfWeek.Tuesday:
                    return DaysOfWeek.Tuesday;
                case DayOfWeek.Wednesday:
                    return DaysOfWeek.Thursday;
                case DayOfWeek.Thursday:
                    return DaysOfWeek.Thursday;
                case DayOfWeek.Friday:
                    return DaysOfWeek.Friday;
                case DayOfWeek.Saturday:
                    return DaysOfWeek.Saturday;
                case DayOfWeek.Sunday:
                    return DaysOfWeek.Sunday;
                default:
                    throw new ArgumentException("current time was not a valid day of the week, somehow");
            }
        }
    }
}