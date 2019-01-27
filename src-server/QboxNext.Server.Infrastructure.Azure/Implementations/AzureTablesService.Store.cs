using Microsoft.Extensions.Logging;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
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

            var entity = new MeasurementEntity(qboxMeasurement);
            _logger.LogInformation($"Inserting measurement for '{qboxMeasurement.SerialNumber}' with RowKey '{entity.RowKey}' into Azure Table '{nameof(_measurementTableSet)}'");

            return await _measurementTableSet.AddAsync(entity).TimeoutAfter(_serverTimeout) != null;
        }

        /// <inheritdoc cref="IAzureTablesService.StoreBatchAsync(IList{QboxMeasurement})"/>
        public async Task<bool> StoreBatchAsync(IList<QboxMeasurement> qboxMeasurements)
        {
            Guard.NotNull(qboxMeasurements, nameof(qboxMeasurements));

            if (qboxMeasurements.Count == 0)
            {
                return true;
            }

            if (qboxMeasurements.Count == 1)
            {
                return await StoreAsync(qboxMeasurements.First());
            }

            string serialNumber = qboxMeasurements.First().SerialNumber; // SerialNumber should be same for all, else Batch Fails

            var entities = qboxMeasurements.Select(qboxMeasurement => new MeasurementEntity(qboxMeasurement)).ToList();

            _logger.LogInformation($"Inserting {qboxMeasurements.Count} measurements for '{serialNumber}' into Azure Table '{nameof(_measurementTableSet)}'");

            return await _measurementTableSet.AddAsync(entities).TimeoutAfter(_serverTimeout) != null;
        }

        public async Task<bool> StoreAsync(QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            var entity = new StateEntity(qboxState);
            _logger.LogInformation($"Inserting state for '{qboxState.SerialNumber}' with RowKey '{entity.RowKey}' into Azure Table '{nameof(_stateTableSet)}'");

            return await _stateTableSet.AddAsync(entity).TimeoutAfter(_serverTimeout) != null;
        }
    }
}