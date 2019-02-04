using System.Net.Http;
using QboxNext.Server.Auth0.Interfaces;

namespace QboxNext.Server.Auth0.Implementations
{
    /// Will be replaced in future by https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore
    /// <seealso cref="IHttpClientFactory" />
    internal class HttpClientFactory : IHttpClientFactory
    {
        /// <summary>
        /// Always return the same static HttpClient. See https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// </summary>
        private static readonly HttpClient Client = new HttpClient();

        /// <summary>
        /// Gets the HTTP client.
        /// </summary>
        /// <returns></returns>
        public HttpClient GetHttpClient()
        {
            return Client;
        }
    }
}