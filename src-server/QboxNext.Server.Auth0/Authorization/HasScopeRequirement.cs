using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using QboxNext.Server.Common.Validation;

namespace QboxNext.Server.Auth0.Authorization
{
    /// <summary>
    /// Create a new authorization requirement called HasScopeRequirement.
    /// </summary>
    /// <seealso cref="IAuthorizationRequirement" />
    internal class HasScopeRequirement : IAuthorizationRequirement
    {
        [PublicAPI]
        public string Issuer { get; }

        [PublicAPI]
        public string Scope { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HasScopeRequirement"/> class.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="issuer">The issuer.</param>
        public HasScopeRequirement(string scope, string issuer)
        {
            Guard.NotNull(scope, nameof(scope));
            Guard.NotNull(issuer, nameof(issuer));

            Scope = scope;
            Issuer = issuer;
        }
    }
}
