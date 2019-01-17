using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Infrastructure.Azure.Interfaces.Public;

namespace QBoxNext.Business.Implementations
{
    internal class DefaultAsyncStorageProviderFactory : IAsyncStorageProviderFactory
    {
        private readonly IMeasurementStoreService _measurementStoreService;

        public DefaultAsyncStorageProviderFactory([NotNull] IMeasurementStoreService measurementStoreService)
        {
            _measurementStoreService = measurementStoreService;
        }

        /// <inheritdoc cref="IAsyncStorageProviderFactory.Create(string, string, int)"/>
        public IAsyncStorageProvider Create(string serialNumber, string productNumber, int counterId)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));

            return new DefaultAsyncStorageProvider(_measurementStoreService, serialNumber, productNumber, counterId);
        }
    }
}