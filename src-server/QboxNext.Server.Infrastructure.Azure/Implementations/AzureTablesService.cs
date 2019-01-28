using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Options;
using System;
using WindowsAzure.Table;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService : IAzureTablesService
    {
        private const int MeasurementPartitionKeyStart = 100000000;
        private static readonly long MaxTicks = DateTime.MaxValue.Ticks + 1;

        private readonly ILogger<AzureTablesService> _logger;
        private readonly ITableSet<RegistrationEntity> _registrationTableSet;
        private readonly ITableSet<MeasurementEntity> _measurementTableSet;
        private readonly ITableSet<StateEntity> _stateTableSet;
        private readonly TimeSpan _serverTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTablesService"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        public AzureTablesService([NotNull] IOptions<AzureTableStorageOptions> options, [NotNull] ILogger<AzureTablesService> logger)
        {
            Guard.NotNull(options, nameof(options));
            Guard.NotNull(logger, nameof(logger));

            _logger = logger;
            _serverTimeout = TimeSpan.FromSeconds(options.Value.ServerTimeout);

            // Create CloudTableClient
            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            // Create table sets
            _registrationTableSet = new TableSet<RegistrationEntity>(client, options.Value.RegistrationsTableName);
            _stateTableSet = new TableSet<StateEntity>(client, options.Value.StatesTableName);
            _measurementTableSet = new TableSet<MeasurementEntity>(client, options.Value.MeasurementsTableName);
        }
    }
}