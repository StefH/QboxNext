﻿using Microsoft.Extensions.Caching.Memory;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QBoxNext.Server.Business.Interfaces.Internal;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class QboxCounterDataCache : IQboxCounterDataCache
    {
        private readonly MemoryCache _cache;

        public QboxCounterDataCache()
        {
            _cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 1024
            });
        }

        /// <inheritdoc cref="IQboxCounterDataCache.GetOrCreateAsync(QboxDataQuery, Func{Task{QboxPagedDataQueryResult{QboxCounterData}}})"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> GetOrCreateAsync(QboxDataQuery query, Func<Task<QboxPagedDataQueryResult<QboxCounterData>>> getDataFunc)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotNull(getDataFunc, nameof(getDataFunc));

            // If the date range contains 'now', always get fresh data
            var now = DateTime.UtcNow;
            if (now >= query.From && now < query.To)
            {
                return await getDataFunc();
            }

            string key = GetKey(query);

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

        private string GetKey(QboxDataQuery query)
        {
            return $"{query.SerialNumber}:{query.From.Ticks}:{query.To.Ticks}:{query.Resolution}";
        }
    }
}
