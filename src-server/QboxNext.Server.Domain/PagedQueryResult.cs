using System.Collections.Generic;

namespace QboxNext.Server.Domain
{
    public class PagedQueryResult<T>
    {
        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}