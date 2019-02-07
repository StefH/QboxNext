using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace QboxNext.Server.Caching.AzureTableStorage.Models
{
    /// <summary>
    /// Represents an item to be stored in the table.
    /// </summary>
    public class CachedItem : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachedItem"/> class.
        /// </summary>
        public CachedItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedItem"/> class.
        /// </summary>
        /// <param name="partitionKey">
        /// The partition key.
        /// </param>
        /// <param name="rowKey">
        /// The row key.
        /// </param>
        /// <param name="data">
        /// The data to store.
        /// </param>
        public CachedItem(string partitionKey, string rowKey, byte[] data = null)
            : base(partitionKey, rowKey)
        {
            Data = data;
        }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the sliding expiration in ticks.
        /// </summary>
        public long? SlidingExpirationTicks { get; set; }

        /// <summary>
        /// Gets or sets the sliding expiration as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <remarks>
        /// Ignored as <see cref="TimeSpan"/> is not supported in Azure table storage.
        /// </remarks>
        [IgnoreProperty]
        public TimeSpan? SlidingExpiration
        {
            get
            {
                if (SlidingExpirationTicks.HasValue)
                {
                    return TimeSpan.FromTicks(SlidingExpirationTicks.Value);
                }

                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    SlidingExpirationTicks = value.Value.Ticks;
                }
            }
        }

        /// <summary>
        /// Gets or sets the absolute expiration date and time.
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets the date and time the item was last accessed.
        /// </summary>
        public DateTimeOffset? LastAccessTime { get; set; }
    }
}
