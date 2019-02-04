namespace QboxNext.Server.Auth0.Interfaces
{
    /// <summary>
    /// A factory to create an Auth0 Client.
    /// </summary>
    internal interface IAuth0ClientFactory
    {
        /// <summary>
        /// Creates an Auth0 Client.
        /// </summary>
        /// <typeparam name="T">Generic interface which implements IAuth0Api</typeparam>
        /// <returns>T</returns>
        T CreateClient<T>() where T : IAuth0Api;
    }
}