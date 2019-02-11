using System;
using Microsoft.Extensions.Logging;

namespace QboxNext.ConsoleApp.Loggers
{
    internal class SimpleConsoleLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            string message = formatter(state, exception);
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception;
            }

            Console.WriteLine(message);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopScope.Instance;
        }

        private class NoopScope : IDisposable
        {
            public static readonly NoopScope Instance = new NoopScope();

            private NoopScope()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}