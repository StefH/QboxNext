using RestEase;

namespace QboxNext.Server.Auth0.Interfaces
{
    /// <summary>
    /// This interface describes the base Auth0 API.
    /// </summary>
    internal interface IAuth0Api
    {
        /// <summary>
        /// The Auth0 domain (host).
        /// </summary>
        [Path("domain", UrlEncode = false)]
        string Domain { get; set; }
    }
}