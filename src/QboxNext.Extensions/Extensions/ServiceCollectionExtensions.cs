using JetBrains.Annotations;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Implementations;
using QboxNext.Extensions.Interfaces.Internal;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Qboxes.Parsing.Factories;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up QboxNext Extensions services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for QboxNext Extensions.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static void AddQboxNext([NotNull] this IServiceCollection services)
        {
            Guard.IsNotNull(services, nameof(services));

            services.AddServices();

            Register();
        }

        private static void AddServices(this IServiceCollection services)
        {
            // Internal
            services.AddScoped<IQboxMessagesLogger, QboxMessagesNullLogger>();
            services.AddScoped<IQboxDataDumpContextFactory, QboxDataDumpContextFactory>();
            services.AddScoped<IQboxNextDataHandlerFactory, QboxNextDataHandlerFactory>();
            services.AddScoped<IMiniPocoFactory, MiniPocoFactory>();
        }

        private static void Register()
        {
            ParserFactory.RegisterAllParsers();
        }
    }
}
