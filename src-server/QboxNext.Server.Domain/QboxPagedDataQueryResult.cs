﻿namespace QboxNext.Server.Domain
{
    public class QboxPagedDataQueryResult<TResult> : PagedQueryResult<TResult>
    {
        public QboxCounterDataValue Overview { get; set; }
    }
}
