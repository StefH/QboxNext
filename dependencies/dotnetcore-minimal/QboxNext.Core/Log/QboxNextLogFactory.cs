using NLog;
using QboxNext.Core.Utils;

namespace QboxNext.Core.Log
{
    /// <summary>
    /// Factory for retrieving the current logger from the datastore. The loggers can be stored
    /// in different datastores depending on the type of implementation (web, console, etc)
    /// This helps to instantiate a logger based on a high level concept and using
    /// it in low level functions.
    /// </summary>
    public static class QboxNextLogFactory
    {
        /// <summary>
        /// Static function that retrieves the logger from the datastore used in the app or if not found it
        /// will return a logger based on the given name.
        /// </summary>
        /// <returns>Nlog Logger</returns>
        public static Logger GetLogger(string loggerName)
        {
            Guard.IsNotNullOrEmpty(loggerName, "loggerName");
            
            if (UnitOfWorkHelper.CurrentDataStore != null && UnitOfWorkHelper.CurrentDataStore[DataStoreName.cLogger] as Logger != null)
            {
                return UnitOfWorkHelper.CurrentDataStore[DataStoreName.cLogger] as Logger;
            }
            return LogManager.GetLogger(loggerName);
        }
    }
}
