using JetBrains.Annotations;
using QboxNext.Common.Validation;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using QBoxNext.Business.Interfaces.Internal;

namespace QBoxNext.Business.Implementations
{
    internal class StorageProviderFactory : IStorageProviderFactory
    {
        private readonly IMeasurementStoreService _measurementStoreService;

        public StorageProviderFactory([NotNull] IMeasurementStoreService measurementStoreService)
        {
            _measurementStoreService = measurementStoreService;
        }

        /// <inheritdoc cref="IStorageProviderFactory.Create(string, string, int)"/>
        public IStorageProviderAsync Create(string serialNumber, string productNumber, int counterId)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.NotNullOrEmpty(productNumber, nameof(productNumber));

            return new CustomStorageProvider(_measurementStoreService, serialNumber, productNumber, counterId);
        }
    }
}