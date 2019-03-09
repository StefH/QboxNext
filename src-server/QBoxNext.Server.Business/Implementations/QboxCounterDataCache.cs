using DistributedCache.AzureTableStorage.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Domain.Utils;
using QBoxNext.Server.Business.Interfaces.Internal;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class QboxCounterDataCache : IQboxCounterDataCache
    {
        private readonly DistributedCacheEntryOptions _defaultDistributedCacheEntryOptions = new DistributedCacheEntryOptions();
        private readonly ILogger<QboxCounterDataCache> _logger;
        private readonly IDistributedCache _cache;

        public QboxCounterDataCache([NotNull] ILogger<QboxCounterDataCache> logger, [NotNull] IDistributedCache cache)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(cache, nameof(cache));

            _logger = logger;
            _cache = cache;
        }

        /// <inheritdoc cref="IQboxCounterDataCache.GetOrCreateAsync(string, QboxDataQuery, Func{Task{QboxPagedDataQueryResult{QboxCounterData}}})"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> GetOrCreateAsync(
            string serialNumber,
            QboxDataQuery query,
            Func<Task<QboxPagedDataQueryResult<QboxCounterData>>> getDataFunc)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.NotNull(query, nameof(query));
            Guard.NotNull(getDataFunc, nameof(getDataFunc));

            if (IsNowOrFuture(query))
            {
                _logger.LogInformation("Query {Query} is a realtime query, getting fresh data from Azure Table Storage", JsonConvert.SerializeObject(query));

                return await getDataFunc();
            }

            var (start, end) = query.Resolution.GetTruncatedTimeFrame(query.From, query.To);
            query.From = start;
            query.To = end;

            string key = ConstructCacheKey(serialNumber, start, end, query.Resolution);

            var cacheEntry = await _cache.GetAsync<QboxPagedDataQueryResult<QboxCounterData>>(key);
            if (cacheEntry == null)
            {
                // Key not in cache, so get data.
                cacheEntry = await getDataFunc();

                // Save data in cache.
                await _cache.SetAsync(key, cacheEntry, _defaultDistributedCacheEntryOptions);
            }

            return cacheEntry;
        }

        private string ConstructCacheKey(string serialNumber, DateTime fromTruncated, DateTime toTruncated, QboxQueryResolution resolution)
        {
            return $"{serialNumber}:{fromTruncated.Ticks}:{toTruncated.Ticks}:{resolution}";
        }

        private static bool IsNowOrFuture(QboxDataQuery query)
        {
            return query.From >= DateTime.UtcNow || query.To > DateTime.UtcNow;
        }
    }
}