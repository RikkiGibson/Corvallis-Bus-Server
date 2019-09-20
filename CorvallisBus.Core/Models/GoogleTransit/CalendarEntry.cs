using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace CorvallisBus.Core.Models.GoogleTransit
{
    class CalendarEntry
    {
        [Name("service_id")]
        public int ServiceId { get; set; }

        [Name("monday")]
        public bool Monday { get; set; }

        [Name("tuesday")]
        public bool Tuesday { get; set; }

        [Name("wednesday")]
        public bool Wednesday { get; set; }

        [Name("thursday")]
        public bool Thursday { get; set; }

        [Name("friday")]
        public bool Friday { get; set; }

        [Name("saturday")]
        public bool Saturday { get; set; }

        [Name("sunday")]
        public bool Sunday { get; set; }

        public DaysOfWeek DaysOfWeek
        {
            get
            {
                var result = DaysOfWeek.None;
                if (Monday) { result |= DaysOfWeek.Monday; }
                if (Tuesday) { result |= DaysOfWeek.Tuesday; }
                if (Wednesday) { result |= DaysOfWeek.Wednesday; }
                if (Thursday) { result |= DaysOfWeek.Thursday; }
                if (Friday) { result |= DaysOfWeek.Friday; }
                if (Saturday) { result |= DaysOfWeek.Saturday; }
                if (Sunday) { result |= DaysOfWeek.Sunday; }
                return result;
            }
        }
    }
}
