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
        private readonly ILogger<AzureTablesService> _logger;
        private readonly TimeSpan _serverTimeout;

        private readonly (string Name, ITableSet<RegistrationEntity> Set) _registrationTable;
        private readonly (string Name, ITableSet<MeasurementEntity> Set) _measurementTable;
        private readonly (string Name, ITableSet<StateEntity> Set) _stateTable;

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
            _registrationTable = (options.Value.RegistrationsTableName, new TableSet<RegistrationEntity>(client, options.Value.RegistrationsTableName));
            _stateTable = (options.Value.StatesTableName, new TableSet<StateEntity>(client, options.Value.StatesTableName));
            _measurementTable = (options.Value.MeasurementsTableName, new TableSet<MeasurementEntity>(client, options.Value.MeasurementsTableName));
        }
    }
}