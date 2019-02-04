using Newtonsoft.Json;

namespace QboxNext.Server.Auth0.Models
{
    /// <summary>
    /// The Auth0 AccessToken Request.
    /// </summary>
    public class Auth0AccessTokenRequest
    {
        /// <summary>
        /// The client_id.
        /// </summary>
        [JsonProperty(PropertyName = "client_id")]
        public string ClientId { get; set; }

        /// <summary>
        /// The client_secret.
        /// </summary>
        [JsonProperty(PropertyName = "client_secret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// The audience.
        /// </summary>
        [JsonProperty(PropertyName = "audience")]
        public string Audience { get; set; }

        /// <summary>
        /// The grant_type.
        /// </summary>
        [JsonProperty(PropertyName = "grant_type")]
        public string GrantType { get; } = "client_credentials";
    }
}