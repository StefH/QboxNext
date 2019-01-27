using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Models.Public;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService
    {
        /// <inheritdoc cref="IAzureTablesService.StoreAsync(QboxMeasurement)"/>
        public async Task<StoreResult> StoreAsync(QboxMeasurement qboxMeasurement)
        {
            Guard.NotNull(qboxMeasurement, nameof(qboxMeasurement));

            var entity = new MeasurementEntity(qboxMeasurement);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting measurement for '{qboxMeasurement.SerialNumber}' with RowKey '{entity.RowKey}' into Azure Table '{_measurementsTable.Name}'");
            var result = await _measurementsTable.ExecuteAsync(insertOperation).TimeoutAfter(_serverTimeout);

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }

        /// <inheritdoc cref="IAzureTablesService.StoreBatchAsync(IList{QboxMeasurement})"/>
        public async Task<IList<StoreResult>> StoreBatchAsync(IList<QboxMeasurement> qboxMeasurements)
        {
            Guard.NotNull(qboxMeasurements, nameof(qboxMeasurements));
            Guard.Condition(qboxMeasurements, q => q.Count <= 100, nameof(qboxMeasurements));

            if (qboxMeasurements.Count == 0)
            {
                return new[] { new StoreResult { HttpStatusCode = 204 } };
            }

            if (qboxMeasurements.Count == 1)
            {
                return new[] { await StoreAsync(qboxMeasurements.First()) };
            }

            string serialNumber = qboxMeasurements.First().SerialNumber; // SerialNumber should be same for all, else Batch Fails

            var entities = qboxMeasurements.Select(qboxMeasurement => new MeasurementEntity(qboxMeasurement)).ToList();
            var batch = new TableBatchOperation();
            foreach (var entity in entities)
            {
                batch.Insert(entity);
            }

            _logger.LogInformation($"Inserting {qboxMeasurements.Count} measurement(s) for '{serialNumber}' into Azure Table '{_measurementsTable.Name}'");
            var results = await _measurementsTable.ExecuteBatchAsync(batch).TimeoutAfter(_serverTimeout);

            return results.Select(result => new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag }).ToList();
        }

        public async Task<StoreResult> StoreAsync(QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            var entity = new StateEntity(qboxState);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting state for '{qboxState.SerialNumber}' with RowKey '{entity.RowKey}' into Azure Table '{_statesTable.Name}'");
            var result = await _statesTable.ExecuteAsync(insertOperation).TimeoutAfter(_serverTimeout);

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }
    }
}