using System;
using System.Collections.Generic;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Exceptions;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Qserver.Core.Factories
{
    /// <summary>
    /// Factory to hold and give out Storage Providers based on setting the StorageProvider attribute of the
    /// Mini (Qbox) in the database.
    /// </summary>
    public class StorageProviderFactory
    {
        /// <summary>
        /// Dictionary holding the storage providers
        /// </summary>
        private static readonly Dictionary<StorageProvider, Type> Dictionary = new Dictionary<StorageProvider, Type>();

        /// <summary>
        /// Registers a type as a Storage provider
        /// </summary>
        /// <param name="provider">Enumeration of possible storage provider id's</param>
        /// <param name="providerType">type of storage provider</param>
        /// <param name="replace">If true replace registered provider</param>
        public static void Register(StorageProvider provider, Type providerType, bool replace = false)
        {
            if (!typeof(IStorageProvider).IsAssignableFrom(providerType))
                throw new StorageException("Storage provider type should implement IStorageProvider");
			if (replace && Dictionary.ContainsKey(provider))
				Dictionary.Remove(provider);
            if (!Dictionary.ContainsKey(provider))
                Dictionary.Add(provider, providerType);
        }


        /// <summary>
        /// Returns a storage provider that was registered earlier.
        /// </summary>
        /// <param name="allowOverwrite"> </param>
        /// <param name="provider">enumeration id for a storage provider</param>
        /// <param name="path">The file path for the storage provider</param>
        /// <param name="counter">Counter nr of the counter that will use the storage provider to store it's data</param>
        /// <param name="precision"> </param>
        /// <param name="id">The id or in our case serial number of the qbox</param>
        /// <param name="storageId"></param>
        /// <returns>Storage provider</returns>
        public static IStorageProvider GetStorageProvider(bool allowOverwrite, StorageProvider provider, string id, string path, int counter, Precision precision, string storageId = null)
        {
            Guard.IsNotNullOrEmpty(id, "id cannot be empty or null");
            Guard.IsNotNullOrEmpty(path, "path cannot be empty or null");

            if (Dictionary.ContainsKey(provider))
            {
                var result = storageId == null ? 
                                    Activator.CreateInstance(Dictionary[provider], id, path, counter, precision, "", false, 7) : 
                                    Activator.CreateInstance(Dictionary[provider], id, path, counter, precision, storageId, false, 7);
                return result as IStorageProvider;
            }

            throw new StorageException($"Storage provider not found {provider}-{id}. Please register the storage before getting it.");
        }
    }
}
