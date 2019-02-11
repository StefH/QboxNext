
namespace QboxNext.Qserver.Core.Interfaces
{
    /// <summary>
    /// Convenience class to hold the references to the Repositories used in the application.
    /// The properties should be set up in a startup of the app, like Global.asax.
    /// </summary>
    public static class ClientRepositories
    {
        /// <summary>
        /// Holds the reference to the Meta data Repository for the Mini
        /// The meta data is used to discover the configuration of the Mini. This includes
        /// the datastore, storage provider and formula information
        /// </summary>
        public static IRepository MetaData = null;

        /// <summary>
        /// The status repository is where the status information for the mini qbox population is stored.
        /// After each dump of a message a Qbox Status object (document) is updated/inserted.
        /// </summary>
        public static IRepository Status = null;

        /// <summary>
        /// The queue holder
        /// </summary>
        public static IQueue<string> Queue = null;

        /// <summary>
        /// Statistics repository, at his moment only views in sql-server will be used
        /// </summary>
        public static IStatistics Statistics = null;
    }
}
