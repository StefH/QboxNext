using System;
using NLog;
using QboxNext.Core.Log;

namespace QboxNext.Qserver.Core.Exceptions
{
    public class StorageException : Exception
    {
        private static readonly Logger Log = QboxNextLogFactory.GetLogger("Qserver");

        public StorageException(string message)
            : base(message)
        {
            Log.Error(message);
        }
    }
}