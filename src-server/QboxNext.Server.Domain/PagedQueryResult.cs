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

        ///// <summary>
        ///// Gets or sets the current page.
        ///// </summary>
        //public int CurrentPage { get; set; }

        ///// <summary>
        ///// Gets or sets the page count.
        ///// </summary>
        //public int PageCount { get; set; }

        ///// <summary>
        ///// Gets or sets the size of the page.
        ///// </summary>
        //public int PageSize { get; set; }
    }
}
