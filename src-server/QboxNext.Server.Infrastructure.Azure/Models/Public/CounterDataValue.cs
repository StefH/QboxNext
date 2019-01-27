using System;

namespace QboxNext.Server.Infrastructure.Azure.Models.Public
{
    public class CounterDataValue
    {
        public DateTime MeasureTime { get; set; }

        public int CounterId { get; set; }

        public double PulseValue { get; set; }
    }
}