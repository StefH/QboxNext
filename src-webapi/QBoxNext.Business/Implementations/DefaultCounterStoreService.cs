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

        public async Task StoreAsync(Guid correlationId, CounterData counterData)
        {
            Guard.IsNotNull(counterData, nameof(counterData));

            var measurement = new Measurement
            {
                CorrelationId = correlationId,
                SerialNumber = counterData.SerialNumber,
                ProductNumber = counterData.ProductNumber,
                CounterId = counterData.CounterId,
                LogTime = DateTime.UtcNow,
                MeasureTime = counterData.MeasureTime,
                PulseValue = counterData.PulseValue,
                PulsesPerUnit = counterData.PulsesPerUnit
            };

            await _dataStoreService.StoreAsync(measurement);
        }
    }
}