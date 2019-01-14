using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Interfaces;
using QBoxNext.Business.Interfaces.Internal;

namespace QBoxNext.Business.Implementations
{
    internal class StorageProviderFactory : IStorageProviderFactory
    {
        /// <inheritdoc cref="IStorageProviderFactory.Create(string, string, int)"/>
        public IStorageProvider Create(string serialNumber, string productNumber, int counterId)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));

            return new CustomStorageProvider(serialNumber, productNumber, counterId);
        }
    }
}