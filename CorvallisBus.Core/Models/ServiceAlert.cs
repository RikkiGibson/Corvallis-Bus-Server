using System;

namespace CorvallisBus.Core.Models
{
    public class ServiceAlert
    {
        public string Title { get; }

        /// <summary>
        /// The publish date of the service alert.
        /// Matches the format "yyyy-MM-dd'T'HH:mm:ssZ".
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
