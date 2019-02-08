using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QBoxNext.Server.Business.Implementations
{
    internal class DefaultCounterStoreService : ICounterStoreService
    {
        private readonly IAzureTablesService _azureTablesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCounterStoreService"/> class.
        /// </summary>
        /// <param name="azureTablesService">The measurement store service.</param>
        public DefaultCounterStoreService([NotNull] IAzureTablesService azureTablesService)
        {
            Guard.IsNotNull(azureTablesService, nameof(azureTablesService));

            _azureTablesService = azureTablesService;
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
                MeasureTime = counterData.MeasureTime,
                PulseValue = counterData.PulseValue
            };

            await _azureTablesService.StoreAsync(measurement);
        }

        /// <inheritdoc cref="ICounterStoreService.StoreAsync(string, IList{CounterData})"/>
        public async Task StoreAsync(string correlationId, IList<CounterData> counters)
        {
            Guard.IsNotNullOrEmpty(correlationId, nameof(correlationId));
            Guard.IsNotNull(counters, nameof(counters));

            var measurements = new List<QboxMeasurement>();
            foreach (var counter in counters)
            {
                var measurement = new QboxMeasurement
                {
                    CorrelationId = correlationId,
                    SerialNumber = counter.SerialNumber,
                    CounterId = counter.CounterId,
                    MeasureTime = counter.MeasureTime,
                    PulseValue = counter.PulseValue
                };
                measurements.Add(measurement);
            }

            await _azureTablesService.StoreBatchAsync(measurements);
        }
    }
}