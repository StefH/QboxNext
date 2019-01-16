namespace QboxNext.Infrastructure.Azure.Models.Public
{
    public class StoreResult
    {
        /// <summary>
        /// The HTTP status code returned by the request.
        /// </summary>
        public int HttpStatusCode { get; set; }

        /// <summary>
        /// The ETag returned with the request results.
        /// </summary>
        public string Etag { get; set; }
    }
}
