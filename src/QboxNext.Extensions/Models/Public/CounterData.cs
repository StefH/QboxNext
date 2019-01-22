using System;
using JetBrains.Annotations;

namespace QboxNext.Extensions.Models.Public
{
    public class CounterData
    {
        [NotNull]
        public string SerialNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public int PulseValue { get; set; }
    }
}