using System;

namespace CorvallisBus.Core.Models
{
    public record ServiceAlert(
        string Title,

        /// <summary>
        /// The publish date of the service alert.
        /// Matches the format "yyyy-MM-dd'T'HH:mm:ssZ".
        /// </summary>
        string PublishDate,

        string Link
        );
}
