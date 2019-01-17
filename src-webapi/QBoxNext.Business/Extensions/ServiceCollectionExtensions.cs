﻿using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Qboxes.Parsing.Factories;
using QBoxNext.Business.Implementations;

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
            Guard.IsNotNull(services, nameof(services));

            services.AddServices();

            Register();
        }

        private static void AddServices(this IServiceCollection services)
        {
            // Internal
            services.AddScoped<IAsyncStorageProviderFactory, DefaultAsyncStorageProviderFactory>();
            services.AddScoped<IAsyncStatusProvider, DefaultAsyncStatusProvider>();

            // Add External
            services.AddQboxNext();
            services.AddAzure();
        }

        private static void Register()
        {
            ParserFactory.RegisterAllParsers();
        }
    }
}
