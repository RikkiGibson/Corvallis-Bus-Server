using System;

namespace CorvallisBus.Core.Models
{
    public class ServiceAlert
    {
        public string Title { get; }

        /// <summary>
        /// The publish date of the service alert.
        /// e.g. 2017-12-18T00:00:00-08:00
        /// </summary>
        public string PublishDate { get; }
        public string Link { get; }

        public ServiceAlert(string title, string publishDate, string link)
        {
            Title = title;
            PublishDate = publishDate;
            Link = link;
        }
    }
}
