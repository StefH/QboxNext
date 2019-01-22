using System;
using JetBrains.Annotations;

namespace QboxNext.Domain
{
    public class QboxMeasurement
    {
        public string CorrelationId { get; set; }

        public DateTime LogTimeStamp { get; set; }

        public int CounterId { get; set; }

        [NotNull]
        public string SerialNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public ulong PulseValue { get; set; }

        public decimal PulsesPerUnit { get; set; }
    }
}