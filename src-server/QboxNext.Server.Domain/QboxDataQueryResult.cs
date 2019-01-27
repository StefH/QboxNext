using System;
using System.Collections.Generic;

namespace QboxNext.Server.Domain
{
    public class QboxDataQueryResult
    {
        public IDictionary<int, DateTime> DateSet { get; set; }

        public IDictionary<int, int> ValueSet { get; set; }
    }
}