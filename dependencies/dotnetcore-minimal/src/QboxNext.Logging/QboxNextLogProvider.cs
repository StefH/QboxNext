using Microsoft.Extensions.Logging;
using System;

namespace QboxNext.Logging
{
    /// <summary>
    /// See https://stackoverflow.com/questions/48676152/asp-net-core-web-api-logging-from-a-static-class
    /// </summary>
    public static class QboxNextLogProvider
    {
        private static ILoggerFactory _loggerFactory;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    throw new InvalidOperationException(
                        "You need to use Dependency Injection to initialize this static set LoggerFactory. " +
                        "Else use `QboxNextLogProvider.LoggerFactory = new NLogLoggerFactory();` " +
                        "or `QboxNextLogProvider.LoggerFactory = new LoggerFactory();` " +
                        "in your main program.");
                }

                return _loggerFactory;
            }

            set => _loggerFactory = value;
        }

        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        public static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
    }
}