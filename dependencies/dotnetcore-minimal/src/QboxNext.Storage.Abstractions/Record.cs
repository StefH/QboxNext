using System;

namespace QboxNext.Storage
{
    public class Record
    {
        public string Id { get; set; }
        public DateTime Time { get; set; }

        /// <summary>
        /// Money in cents.
        /// </summary>
        public decimal Money { get; set; }
        public ushort Quality { get; set; }
        public ulong Raw { get; set; }

        /// <summary>
        /// Usage in kWh or m3.
        /// </summary>
        public decimal KiloWattHour { get; set; }

        public bool IsValidMeasurement { get { return Raw < ulong.MaxValue || Quality > 0; } }

        public Record(ulong raw, decimal kWh, decimal money, ushort quality)
        {            
            Money = money;
            Quality = quality;
            Raw = raw;
            KiloWattHour = kWh;
        }
    }
}