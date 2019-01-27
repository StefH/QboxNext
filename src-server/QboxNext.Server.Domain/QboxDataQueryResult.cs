using System;
using System.Collections.Generic;

namespace QboxNext.Server.Domain
{
    public class QboxDataQueryResult
    {
        public int MinValue { get; set; }

        public int MaxValue { get; set; }

        public IDictionary<DateTime, int> Set { get; set; }
    }
}