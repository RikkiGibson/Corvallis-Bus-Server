using System;

namespace CorvallisBus.Core.Models
{
    /// <param name="PublishDate">
    /// The publish date of the service alert.
    /// Matches the format "yyyy-MM-dd'T'HH:mm:ssZ".
    /// </param>
    public record ServiceAlert(
        string Title,
        string PublishDate,
        string Link);
}
