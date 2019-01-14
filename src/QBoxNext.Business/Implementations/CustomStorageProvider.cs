using Newtonsoft.Json;
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
        private readonly int _counter;
        private static readonly Logger Log = LogManager.GetLogger(nameof(CustomStorageProvider));

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStorageProvider"/> class.
        /// </summary>
        /// <param name="serialNumber">Initializes the serial number property</param>
        /// <param name="filePath">Initializes the path (directory part of the file path)</param>
        /// <param name="counter">Initializes the counter id property</param>
        /// <param name="precision">Initializes the precision that the values are returned in in the GetSeries calls</param>
        /// <param name="storageId">The "second" storageId used in building the file name</param>
        /// <param name="allowOverwrite">Initializes the allowOverwrite property</param>
        /// <param name="nrOfDays">Initialized the number of days the file is initially created for and when a file expansion is made</param>
        public CustomStorageProvider(string serialNumber, string filePath, int counter, Precision precision, string storageId = "", bool allowOverwrite = false, int nrOfDays = 7)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _serialNumber = serialNumber;
            _counter = counter;
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
            Log.Trace($"counter:{_counter} | serialNumber:{_serialNumber} | inMeasureTime: {inMeasureTime} | inPulseValue:{inPulseValue} | inPulsesPerUnit:{inPulsesPerUnit} | inEurocentsPerUnit:{inEurocentsPerUnit} | inRunningTotal:{JsonConvert.SerializeObject(inRunningTotal)}");
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