using JetBrains.Annotations;
using QboxNext.Common.Validation;
using QboxNext.Core;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Statistics;
using System;
using System.Collections.Generic;

namespace QBoxNext.Business.Implementations
{
    internal class CustomStorageProvider : IStorageProvider
    {
        private readonly IMeasurementStoreService _measurementStoreService;
        private readonly string _serialNumber;
        private readonly string _productNumber;
        private readonly int _counterId;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomStorageProvider"/> class.
        /// </summary>
        /// <param name="measurementStoreService">The measurement store service.</param>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="productNumber">The product number.</param>
        /// <param name="counterId">The counter identifier.</param>
        public CustomStorageProvider([NotNull] IMeasurementStoreService measurementStoreService, string serialNumber, string productNumber, int counterId)
        {
            Guard.NotNull(measurementStoreService, nameof(measurementStoreService));
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.NotNullOrEmpty(productNumber, nameof(productNumber));

            _measurementStoreService = measurementStoreService;
            _serialNumber = serialNumber;
            _productNumber = productNumber;
            _counterId = counterId;
        }

        /// <summary>
        /// Sets the value for the given measurement time by calculating the value for kWh, euro and quality index.
        /// </summary>
        /// <param name="inMeasureTime">the time of the measurement</param>
        /// <param name="inPulseValue">the raw pulse value</param>
        /// <param name="inPulsesPerUnit">the formula to calculate the kWh from</param>
        /// <param name="inEurocentsPerUnit">The formula to calculate the value in Euro's</param>
        /// <param name="inRunningTotal">N/A</param>
        public Record SetValue(DateTime inMeasureTime, ulong inPulseValue, decimal inPulsesPerUnit, decimal inEurocentsPerUnit, Record inRunningTotal = null)
        {
            var measurement = new Measurement
            {
                SerialNumber = _serialNumber,
                ProductNumber = _productNumber,
                CounterId = _counterId,
                LogTime = DateTime.UtcNow,
                MeasureTime = inMeasureTime,
                PulseValue = inPulseValue,
                PulsesPerUnit = inPulsesPerUnit,
                EurocentsPerUnit = inEurocentsPerUnit
            };

            _measurementStoreService.Store(measurement);
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