using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Handlers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up QboxNext dependencies in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services required for QboxNext dependencies.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static void AddQbox([NotNull] this IServiceCollection services)
        {
            Guard.IsNotNull(services, nameof(services));

            services.AddServices();
        }

        private static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IQboxNextDataHandlerFactory, QboxNextDataHandlerFactory>();
        }
    }
}