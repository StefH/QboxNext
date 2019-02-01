namespace QboxNext.Server.Domain
{
    public class QboxPagedDataQueryResult<TResult> : PagedQueryResult<TResult>
    {
        public QboxCounterDataValue Extra { get; set; }
    }
}
