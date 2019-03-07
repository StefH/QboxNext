using Microsoft.Extensions.Logging;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService
    {
        /// <inheritdoc cref="IAzureTablesService.StoreAsync(QboxMeasurement)"/>
        public async Task<bool> StoreAsync(QboxMeasurement qboxMeasurement)
        {
            Guard.NotNull(qboxMeasurement, nameof(qboxMeasurement));

            _logger.LogInformation($"Inserting measurement for '{qboxMeasurement.SerialNumber}' into Azure Table '{nameof(_measurementTable)}'");

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
                                PartitionKey = PartitionKeyHelper.ConstructPartitionKey(g.Key.SerialNumber, g.Key.MeasureTime),
                                RowKey = RowKeyHelper.GetRowKey(g.Key.MeasureTime),
                                SerialNumber = g.Key.SerialNumber,
                                MeasureTime = g.Key.MeasureTime,
                                CorrelationId = g.First().CorrelationId,
                                Counter0181 = g.FirstOrDefault(c => c.CounterId == 181)?.PulseValue,
                                Counter0182 = g.FirstOrDefault(c => c.CounterId == 182)?.PulseValue,
                                Counter0281 = g.FirstOrDefault(c => c.CounterId == 281)?.PulseValue,
                                Counter0282 = g.FirstOrDefault(c => c.CounterId == 282)?.PulseValue,
                                Counter2421 = g.FirstOrDefault(c => c.CounterId == 2421)?.PulseValue
                            }).ToList();


            string serialNumber = qboxMeasurements.First().SerialNumber;
            string rowKey = entities.Count == 1 ? entities.First().RowKey : string.Empty;
            _logger.LogInformation("Inserting {Count} measurement(s) for '{SerialNumber}' with RowKey '{RowKey}' into Azure Table '{table}'", entities.Count, serialNumber, rowKey, _measurementTable.Name);

            return await _measurementTable.Set.AddOrUpdateAsync(entities).TimeoutAfter(_serverTimeout) != null;
        }

        public async Task<bool> StoreAsync(QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            var entity = new StateEntity(qboxState);
            _logger.LogInformation("Inserting state for '{SerialNumber}' with RowKey '{RowKey}' into Azure Table '{table}'", qboxState.SerialNumber, entity.RowKey, _stateTable.Name);

            return await _stateTable.Set.AddAsync(entity).TimeoutAfter(_serverTimeout) != null;
        }
    }
}