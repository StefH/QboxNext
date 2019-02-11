namespace QboxNext.Storage
{
    public interface IStorageProviderFactory
    {
        IStorageProvider GetStorageProvider(string serialNumber, int counter, Precision precision, StorageId storageId);
    }
}