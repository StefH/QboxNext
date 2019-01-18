using System;
using JetBrains.Annotations;

namespace QboxNext.Extensions.Models.Public
{
    public class CounterData
    {
        [NotNull]
        public string SerialNumber { get; set; }

        [NotNull]
        public string ProductNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public ulong PulseValue { get; set; }

        public decimal PulsesPerUnit { get; set; }
    }
}