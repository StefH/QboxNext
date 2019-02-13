namespace QboxNext.Storage
{
    public interface IStorageProviderFactory
    {
        IStorageProvider GetStorageProvider(StorageProviderContext context);
    }
}