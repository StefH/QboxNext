using System.Collections.Generic;

namespace QboxNext.Server.Infrastructure.Azure.Models.Public
{
    public class QueryResult
    {
        public IList<CounterDataValue> Values { get; set; }
    }
}