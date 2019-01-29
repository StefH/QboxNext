using System;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.Qserver.Core.Exceptions
{
    public class StorageException : Exception
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger("StorageException");

        public StorageException(string message)
            : base(message)
        {
            Logger.LogError(message);
        }
    }
}