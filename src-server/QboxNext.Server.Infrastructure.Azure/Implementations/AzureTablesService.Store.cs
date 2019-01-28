using Microsoft.Extensions.Logging;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QboxMeasurement = QboxNext.Server.Domain.QboxMeasurement;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService
    {
        /// <inheritdoc cref="IAzureTablesService.StoreAsync(QboxMeasurement)"/>
        public async Task<bool> StoreAsync(QboxMeasurement qboxMeasurement)
        {
            Guard.NotNull(qboxMeasurement, nameof(qboxMeasurement));

            _logger.LogInformation($"Inserting measurement for '{qboxMeasurement.SerialNumber}' into Azure Table '{nameof(_measurementTableSet)}'");

            return await StoreBatchAsync(new[] { qboxMeasurement }).TimeoutAfter(_serverTimeout);
        }

        /// <inheritdoc cref="IAzureTablesService.StoreBatchAsync(IList{QboxMeasurement})"/>
        public async Task<bool> StoreBatchAsync(IList<QboxMeasurement> qboxMeasurements)
        {
            Guard.NotNull(qboxMeasurements, nameof(qboxMeasurements));

            if (qboxMeasurements.Count == 0)
            {
                return true;
            }

            var entities = (from entity in qboxMeasurements
                            group entity by new
                            {
                                entity.SerialNumber,
                                entity.MeasureTime
                            }
            into g
                            select new MeasurementEntity
                            {
                                SerialNumber = g.Key.SerialNumber,
                                PartitionKey = GetPartitionKey(g.Key.SerialNumber, g.Key.MeasureTime),
                                RowKey = GetRowKey(g.Key.MeasureTime, g.FirstOrDefault()?.CorrelationId),
                                MeasureTime = g.Key.MeasureTime,
                                CorrelationId = g.FirstOrDefault()?.CorrelationId,
                                Counter0181 = g.FirstOrDefault(c => c.CounterId == 181)?.PulseValue,
                                Counter0182 = g.FirstOrDefault(c => c.CounterId == 182)?.PulseValue,
                                Counter0281 = g.FirstOrDefault(c => c.CounterId == 281)?.PulseValue,
                                Counter0282 = g.FirstOrDefault(c => c.CounterId == 282)?.PulseValue,
                                Counter2421 = g.FirstOrDefault(c => c.CounterId == 2421)?.PulseValue
                            }).ToList();


            string serialNumber = qboxMeasurements.First().SerialNumber;

            _logger.LogInformation($"Inserting {entities.Count} measurements for '{serialNumber}' into Azure Table '{nameof(_measurementTableSet)}'");

            return await _measurementTableSet.AddOrUpdateAsync(entities).TimeoutAfter(_serverTimeout) != null;
        }

        public async Task<bool> StoreAsync(QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            var entity = new StateEntity(qboxState);
            _logger.LogInformation($"Inserting state for '{qboxState.SerialNumber}' with RowKey '{entity.RowKey}' into Azure Table '{nameof(_stateTableSet)}'");

            return await _stateTableSet.AddAsync(entity).TimeoutAfter(_serverTimeout) != null;
        }

        private static string GetPartitionKey(string serialNumber, DateTime measureTime)
        {
            return $"{serialNumber}:{MeasurementPartitionKeyStart - measureTime.Year * 10000 - measureTime.Month * 100 - measureTime.Day}";
        }

        private static string GetRowKey(DateTime measureTime, string guid)
        {
            return $"{MaxTicks - measureTime.Ticks:d19}:{guid}";
        }
    }
}