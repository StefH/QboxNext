namespace QboxNext.Storage
{
    public class StorageWriteException : StorageException
    {
        public StorageWriteException(string message)
            : base(message)
        {
        }
    }
}
