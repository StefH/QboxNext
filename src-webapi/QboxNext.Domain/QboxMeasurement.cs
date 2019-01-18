using System;
using JetBrains.Annotations;

namespace QboxNext.Domain
{
    public class QboxMeasurement
    {
        public Guid CorrelationId { get; set; }

        public DateTime LogTime { get; set; }

        public int CounterId { get; set; }

        [NotNull] public string SerialNumber { get; set; }

        [NotNull] public string ProductNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public ulong PulseValue { get; set; }

        public decimal PulsesPerUnit { get; set; }
    }
}