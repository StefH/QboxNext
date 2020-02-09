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

namespace QBoxNext.Server.FunctionApp.Services
{
    internal class AzureTableStorageCleaner : IAzureTableStorageCleaner
    {
        private readonly ILogger<AzureTableStorageCleaner> _logger;
        private readonly IOptions<AzureTableStorageCleanerOptions> _options;

        private readonly (string Name, ITableSet<StateEntity> Set) _stateTable;

        public AzureTableStorageCleaner(ILogger<AzureTableStorageCleaner> logger, IOptions<AzureTableStorageCleanerOptions> options)
        {
            _logger = logger;
            _options = options;

            // Create CloudTableClient
            var client = CloudStorageAccount.Parse(options.Value.ConnectionString).CreateCloudTableClient();

            // Create table sets
            _stateTable = (options.Value.StatesTableName, new TableSet<StateEntity>(client, options.Value.StatesTableName));
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
    }
}