using JetBrains.Annotations;
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
        private readonly CloudTable _measurementsTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementStoreService"/> class.
        /// </summary>
        /// <param name="options">The AzureTableStorageOptions.</param>
        public MeasurementStoreService([NotNull] IOptions<AzureTableStorageOptions> options)
        {
            Guard.NotNull(options, nameof(options));

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

            var result = _measurementsTable.ExecuteAsync(insertOperation).Result; // TODO

            return new StoreResult { HttpStatusCode = result.HttpStatusCode, Etag = result.Etag };
        }
    }
}