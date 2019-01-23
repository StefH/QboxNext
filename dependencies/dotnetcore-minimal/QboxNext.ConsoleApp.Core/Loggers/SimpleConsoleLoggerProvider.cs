using Microsoft.Extensions.Logging;

namespace QboxNext.ConsoleApp.Loggers
{
    internal class SimpleConsoleLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new SimpleConsoleLogger();
        }
    }
}