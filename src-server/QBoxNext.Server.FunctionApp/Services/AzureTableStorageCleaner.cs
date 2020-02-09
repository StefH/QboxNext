using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using QboxNext.Server.Common.Extensions;
using QboxNext.Server.Infrastructure.Azure.Utils;
using QBoxNext.Server.FunctionApp.Models;
using QBoxNext.Server.FunctionApp.Options;
using WindowsAzure.Table;
using WindowsAzure.Table.Extensions;

namespace QBoxNext.Server.FunctionApp.Services
{
    internal class AzureTableStorageCleaner : IAzureTableStorageCleaner
    {
        private readonly ILogger<AzureTableStorageCleaner> _logger;
        private readonly IOptions<AzureTableStorageCleanerOptions> _options;
        private readonly TimeSpan _serverTimeout;

        private readonly (string Name, ITableSet<StateEntity> Set) _stateTable;

        public AzureTableStorageCleaner(ILogger<AzureTableStorageCleaner> logger, IOptions<AzureTableStorageCleanerOptions> options)
        {
            _logger = logger;
            _options = options;

            _serverTimeout = TimeSpan.FromSeconds(options.Value.ServerTimeout);

            // Create CloudTableClient
            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            // Create table sets
            _stateTable = (options.Value.StatesTableName, new TableSet<StateEntity>(client, options.Value.StatesTableName));
        }

        public async Task CleanupStatesAsync()
        {
            var fromDate = DateTime.UtcNow.AddMonths(-_options.Value.StatesTableRetentionInMonths);
            string fromRowKey = DateTimeRowKeyHelper.Construct(fromDate);

            _logger.LogInformation("Azure Table '{table}': querying older rows then '{fromDate}' [{fromRowKey}]", _stateTable.Name, fromDate, fromRowKey);

            var rowsToDelete = await _stateTable.Set
                .Where(stateEntity => string.CompareOrdinal(stateEntity.RowKey, fromRowKey) > 0)
                .OrderBy(stateEntity => stateEntity.RowKey)
                .ToListAsync();

            if (rowsToDelete.Count > 0)
            {
                _logger.LogInformation("Azure Table '{table}': {rowsToDelete} rows found from {first} to {last}",
                    _stateTable.Name, rowsToDelete.Count,
                    DateTimeRowKeyHelper.Deconstruct(rowsToDelete.FirstOrDefault()?.RowKey),
                    DateTimeRowKeyHelper.Deconstruct(rowsToDelete.LastOrDefault()?.RowKey)
                );

                if (_options.Value.StatesTableDeleteRows)
                {
                    await _stateTable.Set.RemoveAsync(rowsToDelete).TimeoutAfter(_serverTimeout);

                    _logger.LogInformation("Azure Table '{table}': rows deleted", _stateTable.Name);
                }
            }
            else
            {
                _logger.LogInformation("Azure Table '{table}': no rows deleted", _stateTable.Name);
            }
        }
    }
}
