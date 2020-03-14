using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using QboxNext.Server.Infrastructure.Azure.Utils;
using QBoxNext.Server.FunctionApp.Models;
using QBoxNext.Server.FunctionApp.Options;
using WindowsAzure.Table;
using WindowsAzure.Table.Extensions;
using NLog.Extensions.AzureTables;

namespace QBoxNext.Server.FunctionApp.Services
{
    internal class AzureTableStorageCleaner : IAzureTableStorageCleaner
    {
        private readonly ILogger<AzureTableStorageCleaner> _logger;
        private readonly IOptions<AzureTableStorageCleanerOptions> _options;

        private readonly (string Name, ITableSet<StateEntity> Set) _stateTable;
        private readonly (string Name, ITableSet<LoggingEntity> Set) _loggingTable;

        public AzureTableStorageCleaner(ILogger<AzureTableStorageCleaner> logger, IOptions<AzureTableStorageCleanerOptions> options)
        {
            _logger = logger;
            _options = options;

            // Create CloudTableClient
            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            // Create table sets
            _stateTable = (options.Value.StatesTableName, new TableSet<StateEntity>(client, options.Value.StatesTableName));
            _loggingTable = (options.Value.LoggingTableName, new TableSet<LoggingEntity>(client, options.Value.LoggingTableName));
        }

        public async Task CleanupStatesAsync()
        {
            DateTime fromDate = DateTime.UtcNow.AddMonths(-_options.Value.StatesTableRetentionInMonths);
            string fromRowKey = DateTimeRowKeyHelper.Construct(fromDate);

            _logger.LogInformation("Azure Table '{table}': querying older rows then '{fromDate}' [{fromRowKey}]", _stateTable.Name, fromDate, fromRowKey);

            var rowsToDelete = await _stateTable.Set
                .Where(stateEntity => string.CompareOrdinal(stateEntity.RowKey, fromRowKey) >= 0)
                .ToListAsync();

            var rowsToDeleteOrdered = rowsToDelete.OrderByDescending(e => e.RowKey).ToList();
            int count = rowsToDeleteOrdered.Count;
            if (count > 0)
            {
                string firstRowKey = rowsToDeleteOrdered.First().RowKey;
                string lastRowKey = rowsToDeleteOrdered.Last().RowKey;

                _logger.LogInformation("Azure Table '{table}': {rowsToDelete} rows found from '{first}' [{firstRowKey}] to '{last}' [{lastRowKey}]",
                    _stateTable.Name, count,
                    DateTimeRowKeyHelper.Deconstruct(firstRowKey), firstRowKey,
                    DateTimeRowKeyHelper.Deconstruct(lastRowKey), lastRowKey
                );

                if (_options.Value.StatesTableDeleteRows)
                {
                    await _stateTable.Set.RemoveAsync(rowsToDeleteOrdered);

                    _logger.LogInformation("Azure Table '{table}': {rowsToDelete} rows deleted", _stateTable.Name, count);
                }
                else
                {
                    _logger.LogInformation("Azure Table '{table}': no rows deleted because 'StatesTableDeleteRows' is set to false", _stateTable.Name);
                }
            }
            else
            {
                _logger.LogInformation("Azure Table '{table}': no rows deleted because no rows found", _stateTable.Name);
            }
        }

        public async Task CleanupLoggingAsync()
        {
            DateTime fromDate = DateTime.UtcNow.AddMonths(-_options.Value.LoggingTableRetentionInMonths).Date;
            string fromPartitionKey = NLogPartitionKeyHelper.Construct(fromDate);

            _logger.LogInformation("Azure Table '{table}': querying older rows then '{fromDate:d}' [{fromPartitionKey}]", _loggingTable.Name, fromDate, fromPartitionKey);

            var rowsToDelete = await _loggingTable.Set
                .Where(stateEntity => string.CompareOrdinal(stateEntity.PartitionKey, fromPartitionKey) >= 0)
                .ToListAsync();

            var rowsToDeleteOrdered = rowsToDelete.OrderByDescending(e => e.RowKey).ToList();
            int count = rowsToDeleteOrdered.Count;
            if (count > 0)
            {
                string firstPartitionKey = rowsToDeleteOrdered.First().PartitionKey;
                string lastPartitionKey = rowsToDeleteOrdered.Last().PartitionKey;

                _logger.LogInformation("Azure Table '{table}': {rowsToDelete} rows found from '{first:d}' [{firstPartitionKey}] to '{last:d}' [{lastPartitionKey}]",
                    _loggingTable.Name, count,
                    NLogPartitionKeyHelper.Deconstruct(firstPartitionKey), firstPartitionKey,
                    NLogPartitionKeyHelper.Deconstruct(lastPartitionKey), lastPartitionKey
                );

                if (_options.Value.LoggingTableDeleteRows)
                {
                    await _loggingTable.Set.RemoveAsync(rowsToDeleteOrdered);

                    _logger.LogInformation("Azure Table '{table}': {rowsToDelete} rows deleted", _loggingTable.Name, count);
                }
                else
                {
                    _logger.LogInformation("Azure Table '{table}': no rows deleted because 'LoggingTableDeleteRows' is set to false", _loggingTable.Name);
                }
            }
            else
            {
                _logger.LogInformation("Azure Table '{table}': no rows deleted because no rows found", _loggingTable.Name);
            }
        }
    }
}