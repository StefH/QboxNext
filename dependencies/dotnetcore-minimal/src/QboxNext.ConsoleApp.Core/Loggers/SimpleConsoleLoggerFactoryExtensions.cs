using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace QboxNext.ConsoleApp.Loggers
{
    /// <summary>
    /// Extension methods for the <see cref="ILoggerFactory" /> class.
    /// </summary>
    public static class SimpleConsoleLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a simple console logger named 'SimpleConsole' to the factory. The simple console logger only outputs messages > <see cref="LogLevel.Information" /> and without any layout (templates).
        /// </summary>
        /// <param name="builder">The extension method argument.</param>
        public static ILoggingBuilder AddSimpleConsole(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SimpleConsoleLoggerProvider>());

            return builder;
        }
    }
}