using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CorvallisTransit.Models
{
    /// <summary>
    /// Represents the days of the week in during which a schedule is in effect.
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

        private static DaysOfWeek ToDayOfWeek(string day)
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
        /// Gets all the days of the week contained in the input string using a regular expression.
        /// </summary>
        public static DaysOfWeek GetDaysOfWeek(string days)
        {
            DaysOfWeek result = DaysOfWeek.None;
            foreach (Match match in m_dayOfWeekPattern.Matches(days))
            {
                result |= ToDayOfWeek(match.Value);
            }
            return result;
        }
    }
}