using System;

namespace QboxNext.Server.Domain
{
    public class QboxCounterDataValue
    {
        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public int PulseValue { get; set; }
    }
}