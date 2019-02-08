namespace QboxNext.Server.Domain
{
    public class QboxPagedDataQueryResult<TResult> : PagedQueryResult<TResult>
    {
        public QboxCounterData Overview { get; set; }
    }
}
