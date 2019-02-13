namespace QboxNext.Qserver.Core.Exceptions
{
    public class StorageWriteException : StorageException
    {
        public StorageWriteException(string message)
            : base(message)
        {
        }
    }
}
