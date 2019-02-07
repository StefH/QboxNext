namespace QboxNext.Server.Caching.AzureTableStorage.Interfaces.Internal
{
    internal interface IBinarySerializer
    {
        byte[] ToByteArray(object obj);

        T FromByteArray<T>(byte[] byteArray) where T : class;
    }
}