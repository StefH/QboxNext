using System;

namespace QboxNext.Domain
{
    public class Measurement
    {
        public DateTime LogTime { get; set; }

        public int CounterId { get; set; }

        public string SerialNumber { get; set; }

        public string ProductNumber { get; set; }

        public DateTime MeasureTime { get; set; }

        public ulong PulseValue { get; set; }

        public decimal PulsesPerUnit { get; set; }
    }
}