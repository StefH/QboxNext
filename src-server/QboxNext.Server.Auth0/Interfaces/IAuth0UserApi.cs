using System.Net.Http.Headers;
using System.Threading.Tasks;
using QboxNext.Server.Auth0.Models;
using RestEase;

namespace QboxNext.Server.Auth0.Interfaces
{
    /// <summary>
    /// This interface describes the Auth0 User API and is used by RestEase to create a Rest Client.
    /// </summary>
    internal interface IAuth0UserApi : IAuth0Api
    {
        /// <summary>
        /// Gets or sets the Authorization Header (bearer).
        /// </summary>
        [Header("Authorization")]
        AuthenticationHeaderValue Authorization { get; set; }

        /// <summary>
        /// Get a User.
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>A <see cref="User"/></returns>
        [Get("{domain}users/{userId}")]
        Task<User> GetAsync([Path] string userId);
    }
}
