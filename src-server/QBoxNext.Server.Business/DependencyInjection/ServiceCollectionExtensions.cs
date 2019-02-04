using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.DependencyInjection;
using QBoxNext.Server.Business.Implementations;
using QBoxNext.Server.Business.Interfaces.Internal;
using QBoxNext.Server.Business.Interfaces.Public;

// ReSharper disable once CheckNamespace
namespace QBoxNext.Server.Business.DependencyInjection
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
        public static IServiceCollection AddBusiness([NotNull] this IServiceCollection services)
        {
            Guard.IsNotNull(services, nameof(services));

            services.AddServices();

            return services;
        }

        private static void AddServices(this IServiceCollection services)
        {
            // Internal
            services.AddSingleton<IQboxCounterDataCache, QboxCounterDataCache>();

            services.AddScoped<ICounterStoreService, DefaultCounterStoreService>();
            services.AddScoped<IStateStoreService, DefaultStateStoreService>();
            services.AddScoped<IRegistrationService, DefaultRegistrationService>();
            services.AddScoped<IDataQueryService, DataQueryService>();

            // Add External
            services.AddQboxNextExtensions();
            services.AddAzure();
        }
    }
}
