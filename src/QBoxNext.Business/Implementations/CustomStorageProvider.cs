using NLog;
using QboxNext.Core;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Statistics;
using System;
using System.Collections.Generic;

namespace QBoxNext.Business.Implementations
{
    internal class CustomStorageProvider : IStorageProvider
    {
        private readonly string _serialNumber;
        private readonly int _counterId;
        private static readonly Logger Log = LogManager.GetLogger(nameof(CustomStorageProvider));

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStorageProvider"/> class.
        /// </summary>
        /// <param name="serialNumber">Initializes the serial number property</param>
        /// <param name="counterId">Initializes the counter id property</param>
        public CustomStorageProvider(string serialNumber, int counterId)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _serialNumber = serialNumber;
            _counterId = counterId;
        }

        /// <summary>
        /// Sets the value for the given measurement time by calculating the value for kWh, euro and quality index.
        /// It creates a delta for the pulses and calculates the running total.
        /// It fills the gaps(if any) with averages and adds the quality index. 
        /// </summary>
        /// <param name="inMeasureTime">the time of the measurement</param>
        /// <param name="inPulseValue">the raw pulse value</param>
        /// <param name="inPulsesPerUnit">the formula to calculate the kWh from</param>
        /// <param name="inEurocentsPerUnit">The formula to calculate the value in Euro's</param>
        /// <param name="inRunningTotal">
        /// This value is used for efficiency so the function does not need to try and find the current value.
        /// With large files and long time between set value finding the value will degrade performance.
        /// </param>
        public Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null)
        {
            Log.Trace($"counter:{_counterId} | serialNumber:{_serialNumber} | inMeasureTime: {inMeasureTime} | inPulseValue:{inPulseValue} | inPulsesPerUnit:{inPulsesPerUnit} | inEurocentsPerUnit:{inEurocentsPerUnit}");
            return null;
        }

        public Record FindPrevious(DateTime inMeasurementTime)
        {
            return null;
        }

        public bool GetRecords(DateTime inBegin, DateTime inEnd, Unit inUnit, IList<SeriesValue> ioSlots, bool inNegate)
        {
            throw new NotImplementedException();
        }

        public Record GetValue(DateTime measureTime)
        {
            throw new NotImplementedException();
        }

        public void ReinitializeSlots(DateTime inFrom)
        {
            throw new NotImplementedException();
        }

        public decimal Sum(DateTime begin, DateTime end, Unit eenheid)
        {
            throw new NotImplementedException();
        }

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Nothing to do
        }
        #endregion
    }
}