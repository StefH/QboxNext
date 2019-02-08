using System.Net.Http;

namespace QboxNext.Server.Auth0.Interfaces
{
    /// <summary>
    /// Factory to create a HttpClient.
    /// </summary>
    internal interface IHttpClientFactory
    {
        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns></returns>
        HttpClient GetHttpClient();
    }
}