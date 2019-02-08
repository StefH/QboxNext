using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QboxNext.Server.Auth0.Interfaces;
using QboxNext.Server.Common.Validation;
using RestEase;

namespace QboxNext.Server.Auth0.Implementations
{
    /// <seealso cref="IAuth0ClientFactory" />
    internal class Auth0ClientFactory : IAuth0ClientFactory
    {
        /// <summary>
        /// The CamelCasePropertyNamesContractResolver is required for Auth0.
        /// </summary>
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly IHttpClientFactory _factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Auth0ClientFactory"/> class.
        /// </summary>
        /// <param name="factory">The HttpClientFactory.</param>
        public Auth0ClientFactory(IHttpClientFactory factory)
        {
            Guard.NotNull(factory, nameof(factory));

            _factory = factory;
        }

        /// <summary>
        /// Creates the Rest Client.
        /// </summary>
        /// <typeparam name="T">Generic interface</typeparam>
        /// <returns>T</returns>
        public T CreateClient<T>() where T : IAuth0Api
        {
            return new RestClient(_factory.GetHttpClient())
            {
                JsonSerializerSettings = _serializerSettings
            }.For<T>();
        }
    }
}