using System;
using System.Collections.Generic;

namespace QboxNext.Storage
{
    public interface IStorageProvider : IDisposable
    {
        decimal Sum(DateTime begin, DateTime end, Unit unit);

        /// <summary>
        /// Fills in the specified slots.
        /// </summary>
        /// <remarks>
        /// If unit is kWh:
        /// - when the resolution of the slots is smaller than an hour, power in W is stored in each slot.
        /// - otherwise energy in Wh is stored in each slot.
        /// </remarks>
        bool GetRecords(DateTime begin, DateTime end, Unit unit, IList<SeriesValue> slots, bool negate);

        Record GetValue(DateTime measureTime);

        Record SetValue(DateTime measureTime, ulong pulseValue, decimal pulsesPerUnit, decimal eurocentsPerUnit, Record runningTotal = null);

        void ReinitializeSlots(DateTime from);

        Record FindPrevious(DateTime measurementTime);
    }
}
