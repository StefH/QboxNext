using Newtonsoft.Json;
using NLog;
using QboxNext.Core;
using QboxNext.Core.Log;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Statistics;
using System;
using System.Collections.Generic;
using QboxNext.Core.Utils;

namespace QboxNext.Qserver
{
    public class CustomStorageProvider : IStorageProvider
    {
        private readonly string _serialNumber;
        private readonly int _counter;
        private static readonly Logger Log = QboxNextLogFactory.GetLogger(nameof(CustomStorageProvider));

        public CustomStorageProvider(string serialNumber, string filePath, int counter, Precision precision, string storageId = "", bool allowOverwrite = false, int nrOfDays = 7)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _serialNumber = serialNumber;
            _counter = counter;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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

        public Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null)
        {
            Log.Trace($"counter:{_counter} | serialNumber:{_serialNumber} | inMeasureTime: {inMeasureTime} | inPulseValue:{inPulseValue} | inPulsesPerUnit:{inPulsesPerUnit} | inEurocentsPerUnit:{inEurocentsPerUnit} | inRunningTotal:{JsonConvert.SerializeObject(inRunningTotal)}");
            return null;
        }

        public decimal Sum(DateTime begin, DateTime end, Unit eenheid)
        {
            throw new NotImplementedException();
        }
    }
}
