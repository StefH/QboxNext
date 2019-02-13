using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QboxNext.Storage
{
    /// <summary>
    /// Factory to create a specific <see cref="QboxNext.Storage.IStorageProvider"/>.
    /// </summary>
    public class StorageProviderFactory : IStorageProviderFactory
    {
        private readonly ILogger<StorageProviderFactory> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Type _storageProviderType;

        public StorageProviderFactory(ILogger<StorageProviderFactory> logger, IServiceProvider serviceProvider, Type storageProviderType)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _storageProviderType = storageProviderType ?? throw new ArgumentNullException(nameof(storageProviderType));

            if (!typeof(IStorageProvider).IsAssignableFrom(_storageProviderType) || !_storageProviderType.IsClass)
            {
                throw new ArgumentException($"The specified type does not implement {typeof(IStorageProvider).FullName}.", nameof(storageProviderType));
            }
        }

        public IStorageProvider GetStorageProvider(StorageProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                return (IStorageProvider)ActivatorUtilities.CreateInstance(_serviceProvider, _storageProviderType, context);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Unable to create storage provider {StorageProviderType}.", _storageProviderType.FullName);
                throw new StorageException($"Unable to create storage provider {_storageProviderType.FullName}.", ex);
            }
        }
    }
}
