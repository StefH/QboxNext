using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DefaultCounterStoreService : ICounterStoreService
    {
        private readonly IDataStoreService _dataStoreService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCounterStoreService"/> class.
        /// </summary>
        /// <param name="dataStoreService">The measurement store service.</param>
        public DefaultCounterStoreService([NotNull] IDataStoreService dataStoreService)
        {
            Guard.IsNotNull(dataStoreService, nameof(dataStoreService));

            _dataStoreService = dataStoreService;
        }

        /// <inheritdoc cref="ICounterStoreService.StoreAsync(string, CounterData)"/>
        public async Task StoreAsync(string correlationId, CounterData counterData)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(counterData, nameof(counterData));

            var measurement = new QboxMeasurement
            {
                CorrelationId = correlationId,
                SerialNumber = counterData.SerialNumber,
                CounterId = counterData.CounterId,
                LogTimeStamp = DateTime.UtcNow,
                MeasureTime = counterData.MeasureTime,
                PulseValue = counterData.PulseValue
            };

            await _dataStoreService.StoreAsync(measurement);
        }

        public async Task StoreAsync(IList<(string correlationId, CounterData counterData)> counters)
        {
            Guard.IsNotNull(counters, nameof(counters));

            var measurements = new List<QboxMeasurement>();
            foreach (var counter in counters)
            {
                var measurement = new QboxMeasurement
                {
                    CorrelationId = counter.correlationId,
                    SerialNumber = counter.counterData.SerialNumber,
                    CounterId = counter.counterData.CounterId,
                    LogTimeStamp = DateTime.UtcNow,
                    MeasureTime = counter.counterData.MeasureTime,
                    PulseValue = counter.counterData.PulseValue
                };
                measurements.Add(measurement);
            }

            await _dataStoreService.StoreAsync(measurements);
        }
    }
}