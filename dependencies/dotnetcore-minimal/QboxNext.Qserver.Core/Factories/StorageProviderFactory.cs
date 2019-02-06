using QboxNext.Core;
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Storage;

namespace QboxNext.Qserver.Core.Factories
{
    /// <summary>
    /// Factory to hold and give out Storage Providers based on setting the StorageProvider attribute of the
    /// Mini (Qbox) in the database.
    /// </summary>
    public class StorageProviderFactory : IStorageProviderFactory
    {
        public IStorageProvider GetStorageProvider(string serialNumber, int counter, Precision precision, StorageId storageId)
        {
            return new kWhStorage(serialNumber, Config.DataStorePath, counter, precision, storageId.ToString(), false, 7);
        }
    }
}
