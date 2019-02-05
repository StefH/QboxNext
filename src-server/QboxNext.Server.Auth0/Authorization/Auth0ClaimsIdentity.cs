using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using System.Collections.Generic;
using System.Security.Claims;

namespace QboxNext.Server.Auth0.Authorization
{
    public class Auth0ClaimsIdentity : ClaimsIdentity
    {
        [PublicAPI]
        public string FullName { get; }

        [PublicAPI]
        public IDictionary<string, object> AppMetadata { get; }

        public Auth0ClaimsIdentity([NotNull] IEnumerable<Claim> claims, [NotNull] string fullName, [NotNull] IDictionary<string, object> appMetadata) : base(claims)
        {
            Guard.NotNullOrEmpty(fullName, nameof(fullName));
            Guard.NotNull(appMetadata, nameof(appMetadata));

            FullName = fullName;
            AppMetadata = appMetadata;
        }
    }
}