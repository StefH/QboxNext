using System.Collections.Generic;

namespace QboxNext.Server.Auth0.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class Auth0Options
    {
        /// <summary>
        /// The JWT authority = Domain (https://abc.eu.auth0.com/)
        /// </summary>
        public string JwtAuthority { get; set; }

        /// <summary>
        /// The JWT authority (https://abc)
        /// </summary>
        public string JwtAudience { get; set; }

        /// <summary>
        /// The Audience (https://abc.eu.auth0.com/api/v2/)
        /// </summary>
        public string Audience { get; set; }

        ///// <summary>
        ///// The API identifier (https://abc)
        ///// </summary>
        //public string ApiIdentifier { get; set; }

        /// <summary>
        /// The ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The ClientSecret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The Domain (https://abc.eu.auth0.com)
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The Policies
        /// </summary>
        public List<string> Policies { get; set; }
    }
}