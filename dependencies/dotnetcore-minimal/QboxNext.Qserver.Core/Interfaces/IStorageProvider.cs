using System;
using System.Collections.Generic;
using QboxNext.Core;
using QboxNext.Qserver.Core.Statistics;

namespace QboxNext.Qserver.Core.Interfaces
{
    public interface IStorageProvider: IDisposable
    {
        decimal Sum(DateTime begin, DateTime end, Unit eenheid);

		/// <summary>
		/// Fills in the specified slots.
		/// </summary>
		/// <param name="inBegin"></param>
		/// <param name="inEnd"></param>
		/// <param name="inUnit"></param>
		/// <param name="ioSlots"></param>
		/// <param name="inNegate"></param>
		/// <returns></returns>
		/// <remarks>
		/// If unit is kWh:
		/// - when the resolution of the slots is smaller than an hour, power in W is stored in each slot.
		/// - otherwise energy in Wh is stored in each slot.
		/// </remarks>
        bool GetRecords(DateTime inBegin, DateTime inEnd, Unit inUnit, IList<SeriesValue> ioSlots, bool inNegate);
        
        Record GetValue(DateTime measureTime);
        Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null);
		void ReinitializeSlots(DateTime inFrom);
	    Record FindPrevious(DateTime inMeasurementTime);
    }


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
