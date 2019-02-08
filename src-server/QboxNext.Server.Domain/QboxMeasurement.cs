using JetBrains.Annotations;
using System;

namespace QboxNext.Server.Domain
{
    public class QboxMeasurement
    {
        public string CorrelationId { get; set; }

        public int CounterId { get; set; }

        [NotNull]
        public string SerialNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public int PulseValue { get; set; }
    }
}