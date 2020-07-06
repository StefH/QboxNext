using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QboxNext.Server.Domain
{
    public class QboxPagedDataQueryResult<TResult> : PagedQueryResult<TResult>
    {
        public QboxCounterData Overview { get; set; }
    }

    /// <summary>
    /// This is needed for gRpc.
    /// </summary>
    [DataContract]
    public class QboxPagedCounterDataResult
    {
        [DataMember(Order = 1)]
        public int Count { get; set; }

        [DataMember(Order = 2)]
        public IEnumerable<QboxCounterData> Items { get; set; }

        [DataMember(Order = 3)]
        public QboxCounterData Overview { get; set; }
    }
}
