using System.Collections.Generic;

namespace QboxNext.Server.Auth0.Options
{
    public class Auth0Options
    {
        /// <summary>
        /// Gets the Audience (https://abc.eu.auth0.com/api/v2/)
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets the API identifier (https://abc)
        /// </summary>
        public string ApiIdentifier { get; set; }

        /// <summary>
        /// Gets the ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets the ClientSecret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets the Domain (https://abc.eu.auth0.com)
        /// </summary>
        public string Domain { get; set; }

        public List<string> Policies { get; set; }
    }
}