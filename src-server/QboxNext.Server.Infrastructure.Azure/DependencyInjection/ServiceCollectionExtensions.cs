using JetBrains.Annotations;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Infrastructure.Azure.Implementations;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up Azure services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for Azure.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static void AddAzure([NotNull] this IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services));

            services.AddOptions();

            services.AddServices();
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IAzureTablesService, AzureTablesService>();
        }
    }
}
