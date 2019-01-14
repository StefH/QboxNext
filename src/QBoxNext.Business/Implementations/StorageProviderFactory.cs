using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Interfaces;
using QBoxNext.Business.Interfaces.Internal;

namespace QBoxNext.Business.Implementations
{
    internal class StorageProviderFactory : IStorageProviderFactory
    {
        /// <inheritdoc cref="IStorageProviderFactory.Create(string, int)"/>
        public IStorageProvider Create(string serialNumber, int counterId)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            return new CustomStorageProvider(serialNumber, counterId);
        }
    }
}