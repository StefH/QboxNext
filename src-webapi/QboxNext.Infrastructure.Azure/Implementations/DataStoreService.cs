using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Common.Extensions;
using QboxNext.Core.Utils;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Infrastructure.Azure.Models.Internal;
using QboxNext.Infrastructure.Azure.Models.Public;
using QboxNext.Infrastructure.Azure.Options;
using System;
using System.Threading.Tasks;

namespace QboxNext.Infrastructure.Azure.Implementations
{
    internal class DataStoreService : IDataStoreService
    {
        private readonly ILogger<DataStoreService> _logger;
        private readonly CloudTable _measurementsTable;
        private readonly TimeSpan _serverTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataStoreService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public DataStoreService([NotNull] IOptions<AzureTableStorageOptions> options, [NotNull] ILogger<DataStoreService> logger)
        {
            Guard.IsNotNull(options, nameof(options));
            Guard.IsNotNull(logger, nameof(logger));

            _logger = logger;
            _serverTimeout = TimeSpan.FromSeconds(options.Value.ServerTimeout);

            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            _measurementsTable = client.GetTableReference(options.Value.MeasurementsTableName);
        }

        /// <inheritdoc cref="IDataStoreService.StoreAsync(Measurement)"/>
        public async Task<StoreResult> StoreAsync(Measurement measurement)
        {
            Guard.IsNotNull(measurement, nameof(measurement));

            var entity = new MeasurementEntity(measurement);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting measurement for entity '{entity.PartitionKey}' into Azure Table '{_measurementsTable.Name}'");
            var result = await _measurementsTable.ExecuteAsync(insertOperation).TimeoutAfter(_serverTimeout);

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }
    }
}