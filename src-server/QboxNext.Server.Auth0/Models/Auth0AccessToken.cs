using Newtonsoft.Json;

namespace QboxNext.Server.Auth0.Models
{
    /// <summary>
    /// The Auth0 AccessToken.
    /// </summary>
    public class Auth0AccessToken
    {
        /// <summary>
        /// The token type.
        /// </summary>
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// The scope(s).
        /// </summary>
        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// Expiration from this token in milliseconds.
        /// </summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }
    }
}