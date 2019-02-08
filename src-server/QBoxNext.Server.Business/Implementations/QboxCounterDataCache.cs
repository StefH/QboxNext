using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly ILogger<QboxCounterDataCache> _logger;
        private readonly MemoryCache _cache;

        public QboxCounterDataCache([NotNull] ILogger<QboxCounterDataCache> logger)
        {
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 10000
            });
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

            if (IsRealTime(query))
            {
                _logger.LogInformation("Query {Query} is a realtime query.", JsonConvert.SerializeObject(query));

                return await getDataFunc();
            }

            var (start, end) = query.Resolution.GetTruncatedTimeFrame(query.From, query.To);
            query.From = start;
            query.To = end;

            string key = GetKey(serialNumber, start, end, query.Resolution);

            if (!_cache.TryGetValue(key, out QboxPagedDataQueryResult<QboxCounterData> cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = await getDataFunc();

                int size = (int)(query.To - query.From).TotalDays;

                // Set cache entry size by extension method.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(size);

                // Save data in cache.
                _cache.Set(key, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        private string GetKey(string serialNumber, DateTime fromTruncated, DateTime toTruncated, QboxQueryResolution resolution)
        {
            return $"{serialNumber}:{fromTruncated.Ticks}:{toTruncated.Ticks}:{resolution}";
        }

        private static bool IsRealTime(QboxDataQuery query)
        {
            return query.From >= DateTime.UtcNow && DateTime.UtcNow < query.From;
        }
    }
}