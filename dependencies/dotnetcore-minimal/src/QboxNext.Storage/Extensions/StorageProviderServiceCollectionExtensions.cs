using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace QboxNext.Storage.Extensions
{
    public static class StorageProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a storage provider implementing <see cref="IStorageProvider"/>. Only a single storage provider is supported at the moment.
        /// </summary>
        /// <typeparam name="TStorageProvider">The type implementing <see cref="IStorageProvider"/>.</typeparam>
        /// <param name="services">The service container.</param>
        /// <returns>Returns the service container.</returns>
        public static IServiceCollection AddStorageProvider<TStorageProvider>(this IServiceCollection services)
            where TStorageProvider : class, IStorageProvider
        {
            services.TryAddSingleton<IStorageProviderFactory>(s => ActivatorUtilities.CreateInstance<StorageProviderFactory>(s, typeof(TStorageProvider)));

            return services;
        }
    }
}
