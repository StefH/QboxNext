using System;

namespace QboxNext.Server.Domain
{
    public class QboxCounterDataValue
    {
        public string Label { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public int AveragePulseValue { get; set; }

        public int Min { get; set; }

        public int Max { get; set; }

        public int Delta { get; set; }
    }
}