﻿using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Common.Validation;
using QboxNext.Domain;
using QboxNext.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Infrastructure.Azure.Models.Internal;
using QboxNext.Infrastructure.Azure.Models.Public;
using QboxNext.Infrastructure.Azure.Options;

namespace QboxNext.Infrastructure.Azure.Implementations
{
    internal class MeasurementStoreService : IMeasurementStoreService
    {
        private readonly ILogger<MeasurementStoreService> _logger;
        private readonly CloudTable _measurementsTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStoreService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public MeasurementStoreService([NotNull] IOptions<AzureTableStorageOptions> options, [NotNull] ILogger<MeasurementStoreService> logger)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;

            var storageAccount = CloudStorageAccount.Parse(options.Value.ConnectionString);

            var client = storageAccount.CreateCloudTableClient();

            _measurementsTable = client.GetTableReference(options.Value.MeasurementsTableName);
        }

        /// <inheritdoc cref="IMeasurementStoreService.Store(Measurement)"/>
        public StoreResult Store(Measurement measurement)
        {
            Guard.NotNull(measurement, nameof(measurement));

            var entity = new MeasurementEntity(measurement);

            var insertOperation = TableOperation.Insert(entity);

            _logger.LogInformation($"Inserting measurement for entity '{entity.PartitionKey}' into Azure Table '{_measurementsTable.Name}'");
            var result = _measurementsTable.ExecuteAsync(insertOperation).Result; // TODO ?

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }
    }
}