using QboxNext.Server.Caching.AzureTableStorage.Interfaces.Internal;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace QboxNext.Server.Caching.AzureTableStorage.Implementations
{
    internal class BinarySerializer : IBinarySerializer
    {
        public byte[] ToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public T FromByteArray<T>(byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(byteArray))
            {
                return binaryFormatter.Deserialize(memoryStream) as T;
            }
        }
    }
}