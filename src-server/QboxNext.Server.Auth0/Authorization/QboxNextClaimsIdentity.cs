using JetBrains.Annotations;
using QboxNext.Server.Auth0.Models;
using QboxNext.Server.Common.Validation;
using System.Collections.Generic;
using System.Security.Claims;

namespace QboxNext.Server.Auth0.Authorization
{
    public class QboxNextClaimsIdentity : ClaimsIdentity
    {
        public string FullName { get; }

        public string QboxSerialNumber { get; }

        public QboxNextClaimsIdentity([NotNull] IEnumerable<Claim> claims, [NotNull] string fullName, [NotNull] AppMetadata appMetadata) : base(claims)
        {
            Guard.NotNullOrEmpty(fullName, nameof(fullName));
            Guard.NotNull(appMetadata, nameof(appMetadata));
            Guard.NotNullOrEmpty(appMetadata.QboxSerialNumber, nameof(appMetadata.QboxSerialNumber));

            FullName = fullName;
            QboxSerialNumber = appMetadata.QboxSerialNumber;
        }
    }
}