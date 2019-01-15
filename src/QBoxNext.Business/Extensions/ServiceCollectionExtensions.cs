using JetBrains.Annotations;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Common.Validation;
using QboxNext.Qboxes.Parsing.Factories;
using QBoxNext.Business.Implementations;
using QBoxNext.Business.Interfaces.Internal;
using QBoxNext.Business.Interfaces.Public;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up QboxNext Business services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for QboxNext Business.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static void AddBusiness([NotNull] this IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services));

            services.AddServices();

            Register();
        }

        private static void AddServices(this IServiceCollection services)
        {
            // Internal
            services.AddScoped<IQboxMessagesLogger, QboxMessagesNullLogger>();
            services.AddScoped<IStorageProviderFactory, StorageProviderFactory>();
            services.AddScoped<IQboxDataDumpContextFactory, QboxDataDumpContextFactory>();
            services.AddScoped<IMiniPocoFactory, MiniPocoFactory>();

            // Add Azure
            services.AddAzure();
        }

        private static void Register()
        {
            ParserFactory.RegisterAllParsers();
        }
    }
}
