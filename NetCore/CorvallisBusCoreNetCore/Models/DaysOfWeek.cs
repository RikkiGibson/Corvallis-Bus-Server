using System;
using System.Text.RegularExpressions;

namespace CorvallisBusCoreNetCore.Models
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
        NightOwl = Thursday | Friday | Saturday,
    }

    public static class DaysOfWeekUtils
    {
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
        /// Returns a value indicating whether the provided DaysOfWeek value is applicable today.
        /// </summary>
        public static bool IsToday(DaysOfWeek days, DateTimeOffset currentTime)
        {
            // special handling for Night Owl so that its schedule is visible after midnight
            // i.e., if it's 2AM on Sunday, we still consider "today" to be Saturday.
            var time = (days == DaysOfWeek.NightOwl)
                ? currentTime.AddHours(-4)
                : currentTime;

            return (ToDaysOfWeek(time.DayOfWeek) & days) != DaysOfWeek.None;
        }
    }
}