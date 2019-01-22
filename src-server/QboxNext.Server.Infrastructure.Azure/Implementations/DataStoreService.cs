using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Models.Public;
using QboxNext.Server.Infrastructure.Azure.Options;
using System;
using System.Threading.Tasks;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal class DataStoreService : IDataStoreService
    {
        private readonly ILogger<DataStoreService> _logger;
        private readonly CloudTable _measurementsTable;
        private readonly CloudTable _statesTable;
        private readonly TimeSpan _serverTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStoreService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DataStoreService([NotNull] IOptions<AzureTableStorageOptions> options, [NotNull] ILogger<DataStoreService> logger)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;
            _serverTimeout = TimeSpan.FromSeconds(options.Value.ServerTimeout);

            // Create CloudTableClient
            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            // Get reference to the tables
            _measurementsTable = client.GetTableReference(options.Value.MeasurementsTableName);
            _statesTable = client.GetTableReference(options.Value.StatesTableName);
        }

        /// <inheritdoc cref="IDataStoreService.StoreAsync(QboxMeasurement)"/>
        public async Task<StoreResult> StoreAsync(QboxMeasurement qboxMeasurement)
        {
            Guard.NotNull(qboxMeasurement, nameof(qboxMeasurement));

            var entity = new MeasurementEntity(qboxMeasurement);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting measurement for '{qboxMeasurement.SerialNumber}' with key '{entity.RowKey}' into Azure Table '{_measurementsTable.Name}'");
            var result = await _measurementsTable.ExecuteAsync(insertOperation).TimeoutAfter(_serverTimeout);

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }

        public async Task<StoreResult> StoreAsync(QboxState qboxState)
        {
            Guard.NotNull(qboxState, nameof(qboxState));

            var entity = new StateEntity(qboxState);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting state for '{qboxState.SerialNumber}' with key '{entity.RowKey}' into Azure Table '{_statesTable.Name}'");
            var result = await _statesTable.ExecuteAsync(insertOperation).TimeoutAfter(_serverTimeout);

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }
    }
}