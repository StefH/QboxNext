using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Domain;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using System;
using System.Threading.Tasks;

namespace QBoxNext.Business.Implementations
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
                ProductNumber = counterData.ProductNumber,
                CounterId = counterData.CounterId,
                LogTimeStamp = DateTime.UtcNow,
                MeasureTime = counterData.MeasureTime,
                PulseValue = counterData.PulseValue,
                PulsesPerUnit = counterData.PulsesPerUnit
            };

            await _dataStoreService.StoreAsync(measurement);
        }
    }
}