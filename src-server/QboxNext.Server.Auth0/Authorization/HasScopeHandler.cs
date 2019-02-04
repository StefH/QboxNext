using Microsoft.AspNetCore.Authorization;
using QboxNext.Server.Auth0.Interfaces;
using QboxNext.Server.Auth0.Models;
using QboxNext.Server.Common.Validation;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using QboxNext.Server.Auth0.Options;

namespace QboxNext.Server.Auth0.Authorization
{
    /// <summary>
    /// This requirement checks if the scope claim issued by your Auth0 tenant is present.
    /// If the scope claim exists, the requirement checks if the scope claim contains the requested scope.
    /// 
    /// Based on <see cref="!:https://github.com/auth0-samples/auth0-aspnetcore-webapi-samples/blob/master/Quickstart/01-Authorization/HasScopeHandler.cs">HasScopeHandler.cs</see>.
    /// </summary>
    /// <seealso cref="AuthorizationHandler{HasScopeRequirement}" />
    internal class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        private static string NameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private static string ScopeType = "scope";

        private readonly IAuth0ClientFactory _factory;
        private readonly Auth0Options _options;

        public HasScopeHandler(IAuth0ClientFactory factory, IOptions<Auth0Options> options)
        {
            Guard.NotNull(factory, nameof(factory));
            Guard.NotNull(options, nameof(options));

            _factory = factory;
            _options = options.Value;
        }

        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        /// <returns>Task</returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            Guard.NotNull(context, nameof(context));
            Guard.NotNull(requirement, nameof(requirement));

            if (!UserHasRequiredScopeRequirement(context, requirement))
            {
                // TODO logging
                return;
            }

            var nameidentifierClaim = GetNameIdentifierClaim(context);
            if (nameidentifierClaim == null)
            {
                // TODO logging
                return;
            }

            var user = await GetUserAsync(nameidentifierClaim.Value);
            if (user?.AppMetadata == null)
            {
                // TODO logging
                return;
            }

            AddUserIdentityToContext(context, requirement, user);
        }

        /// <summary>
        /// Check if the user has the correct scope requirement claim.
        /// </summary>
        private bool UserHasRequiredScopeRequirement(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // Check if the user has the 'scope' claim.
            if (!context.User.HasClaim(c => c.Type == ScopeType && c.Issuer == requirement.Issuer))
            {
                return false;
            }

            // Split the scopes string into an array
            string[] scopes = context.User.FindFirst(c => c.Type == ScopeType && c.Issuer == requirement.Issuer).Value.Split(' ');

            // Check if the scope array contains the required scope
            return scopes.Any(s => s == requirement.Scope);
        }

        /// <summary>
        /// Get the 'nameidentifier' claim for the user.
        /// </summary>
        private Claim GetNameIdentifierClaim(AuthorizationHandlerContext context)
        {
            // Claim "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" has the value (user-id `auth0|5b02c858dc67155d2b5f7fdc`)
            return context.User.Claims.FirstOrDefault(c => c.Type == NameIdentifierClaimType);
        }

        private async Task<User> GetUserAsync(string userId)
        {
            var tokenClient = _factory.CreateClient<IAuth0TokenApi>();
            tokenClient.Domain = _options.Domain;

            var request = new Auth0AccessTokenRequest
            {
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Audience = _options.Audience
            };
            var token = await tokenClient.GetTokenAsync(request);

            var userClient = _factory.CreateClient<IAuth0UserApi>();
            userClient.Domain = $"{_options.Domain}api/v2/";
            userClient.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

            return await userClient.GetAsync(userId);
        }

        /// <summary>
        /// Adds the user identity to the context and mark the specified requirement as being successfully evaluated.
        /// </summary>
        private void AddUserIdentityToContext(AuthorizationHandlerContext context, HasScopeRequirement requirement, User user)
        {
            var identity = new QboxNextClaimsIdentity(context.User.Claims, user.FullName, user.AppMetadata);
            context.User.AddIdentity(identity);

            context.Succeed(requirement);
        }
    }
}
