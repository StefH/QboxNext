using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Server.Caching.AzureTableStorage.Models;
using QboxNext.Server.Caching.AzureTableStorage.Options;
using QboxNext.Server.Common.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QboxNext.Server.Caching.AzureTableStorage.Implementations
{
    /// <summary>
    /// An <see cref="IDistributedCache"/> implementation to cache data in Azure table storage.
    /// </summary>
    /// <seealso cref="IDistributedCache"/>.
    public class AzureTableStorageCache : IDistributedCache
    {
        /// <summary>
        /// The storage table name.
        /// </summary>
        private readonly AzureTableStorageCacheOptions _options;

        /// <summary>
        /// The storage table client.
        /// </summary>
        private CloudTableClient _tableClient;

        /// <summary>
        /// The storage table.
        /// </summary>
        private CloudTable _table;


        public AzureTableStorageCache([NotNull] IOptions<AzureTableStorageCacheOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            _options = options.Value;

            Connect();
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        /// <returns>
        /// A <see cref="byte[]"/>.
        /// </returns>
        public byte[] Get(string key)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            return GetAsync(key).Result;
        }

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        /// <param name="token">
        /// Optional: The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// A <see cref="byte[]"/>.
        /// </returns>
        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            await RefreshAsync(key, token);

            CachedItem item = await RetrieveAsync(key);
            return item?.Data;
        }

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any).
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public void Refresh(string key)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            RefreshAsync(key).Wait();
        }

        /// <summary>
        /// Refreshes a value in the cache based on its key, resetting its sliding expiration
        /// timeout (if any).
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        /// <param name="token">
        /// Optional: The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            CachedItem item = await RetrieveAsync(key);
            if (item != null)
            {
                if (ShouldDelete(item))
                {
                    await RemoveAsync(item, token);
                    return;
                }

                // In theory setting the ETag shouldn't be required but we end up
                // with 'Precondition Failed' errors otherwise.
                item.ETag = "*";
                item.LastAccessTime = DateTimeOffset.UtcNow;

                TableOperation operation = TableOperation.Replace(item);
                await _table.ExecuteAsync(operation);
            }
        }

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        public void Remove(string key)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            RemoveAsync(key).Wait();
        }

        /// <summary>
        /// Removes the value with the given key.
        /// </summary>
        /// <param name="key">
        /// A string identifying the requested value.
        /// </param>
        /// <param name="token">
        /// Optional: The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            CachedItem item = RetrieveAsync(key).Result;
            if (item != null)
            {
                TableOperation operation = TableOperation.Delete(item);
                return _table.ExecuteAsync(operation);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        /// <param name="item">
        /// The <see cref="CachedItem"/>.
        /// </param>
        /// <param name="token">
        /// Optional: The <see cref="CancellationToken"/>.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public Task RemoveAsync(CachedItem item, CancellationToken token = default(CancellationToken))
        {
            TableOperation operation = TableOperation.Delete(item);
            return _table.ExecuteAsync(operation);
        }

        /// <summary>
        /// Sets a value with the given key.
        /// </summary>
        /// <param name="key">
        /// Sets a value with the given key.
        /// </param>
        /// <param name="value">
        /// The value to set in the cache.
        /// </param>
        /// <param name="options">
        /// The cache options for the value.
        /// </param>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            Guard.NotNullOrEmpty(key, nameof(key));
            Guard.NotNullOrEmpty(value, nameof(value));

            SetAsync(key, value, options).Wait();
        }

        /// <summary>
        /// Sets a value with the given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The options.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">AbsoluteExpiration - The absolute expiration value must be in the future.</exception>
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            Guard.NotNullOrEmpty(key, nameof(key));
            Guard.NotNullOrEmpty(value, nameof(value));

            DateTimeOffset? absoluteExpiration = null;
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = currentTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value <= currentTime)
                {
                    throw new ArgumentOutOfRangeException(
                       nameof(options.AbsoluteExpiration),
                       options.AbsoluteExpiration.Value,
                       "The absolute expiration value must be in the future.");
                }

                absoluteExpiration = options.AbsoluteExpiration;
            }

            var item = new CachedItem(_options.PartitionKey, key, value)
            {
                LastAccessTime = currentTime
            };

            if (absoluteExpiration.HasValue)
            {
                item.AbsoluteExpiration = absoluteExpiration;
            }

            if (options.SlidingExpiration.HasValue)
            {
                item.SlidingExpiration = options.SlidingExpiration;
            }

            TableOperation operation = TableOperation.InsertOrReplace(item);
            return _table.ExecuteAsync(operation);
        }

        /// <summary>
        /// Connect to the Azure storage account and gets the table reference.
        /// </summary>
        private void Connect()
        {
            ConnectAsync().Wait();
        }

        /// <summary>
        /// Connect to the Azure storage account and gets the table reference.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        private async Task ConnectAsync()
        {
            if (_tableClient == null)
            {
                _tableClient = CloudStorageAccount.Parse(_options.ConnectionString).CreateCloudTableClient();
            }

            if (_table == null)
            {
                _table = _tableClient.GetTableReference(_options.TableName);
                await _table.CreateIfNotExistsAsync();
            }
        }

        /// <summary>
        /// A <see cref="Task"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>
        /// A <see cref="Task{CachedItem}"/>.
        /// </returns>
        private async Task<CachedItem> RetrieveAsync(string key)
        {
            TableOperation operation = TableOperation.Retrieve<CachedItem>(_options.PartitionKey, key);
            TableResult result = await _table.ExecuteAsync(operation);
            CachedItem data = result?.Result as CachedItem;
            return data;
        }

        /// <summary>
        /// Checks whether the cached item should be deleted based on the absolute or sliding expiration values.
        /// </summary>
        /// <param name="item">
        /// The <see cref="CachedItem"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the item should be deleted, <c>false</c> otherwise.
        /// </returns>
        private bool ShouldDelete(CachedItem item)
        {
            DateTimeOffset currentTime = DateTimeOffset.UtcNow;
            if (item.AbsoluteExpiration != null && item.AbsoluteExpiration.Value <= currentTime)
            {
                return true;
            }

            if (item.SlidingExpiration.HasValue &&
                item.LastAccessTime.HasValue &&
                item.LastAccessTime.Value.Add(item.SlidingExpiration.Value) < currentTime)
            {
                return true;
            }

            return false;
        }
    }
}