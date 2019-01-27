using Microsoft.WindowsAzure.Storage.Table;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Models.Public;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService
    {
        /// <inheritdoc cref="IAzureTablesService.IsValidRegistrationAsync(string)"/>
        public async Task<bool> IsValidRegistrationAsync(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                return false;
            }

            var retrieve = TableOperation.Retrieve(serialNumber, serialNumber);

            var retrieveResult = await _registrationsTable.ExecuteAsync(retrieve);

            return retrieveResult?.Result != null;
        }

        public async Task<QueryResult> QueryDataAsync(string serialNumber, int[] counterIds, DateTime? from, DateTime? to)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            // Create a query: in this example I use the DynamicTableEntity class
            //var query = _measurementsTable.CreateQuery<DynamicTableEntity>()
            //    .Where(d => d.PartitionKey == "partition1"
            //                && d.Timestamp >= startDate && d.Timestamp <= endDate);

            //// Execute the query
            //var result = query.ToList();


            string partitionKeyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, serialNumber);
            string counterIdFilter = TableQuery.GenerateFilterConditionForInt("CounterId", QueryComparisons.NotEqual, 0);
            string fromFilter = TableQuery.GenerateFilterConditionForDate("MeasureTime", QueryComparisons.GreaterThanOrEqual, new DateTime(2019, 1, 1));
            string toFilter = TableQuery.GenerateFilterConditionForDate("MeasureTime", QueryComparisons.LessThan, new DateTime(2019, 1, 31));

            string combinedFilterKeyAndCounter = TableQuery.CombineFilters(partitionKeyFilter, TableOperators.And, counterIdFilter);
            string combinedDateFilter = TableQuery.CombineFilters(fromFilter, TableOperators.And, toFilter);

            string combined = TableQuery.CombineFilters(combinedFilterKeyAndCounter, TableOperators.And, combinedDateFilter);


            var projectionQuery = new TableQuery<MeasurementEntity>()
                .Select(new[] { "MeasureTime", "CounterId", "PulseValue" })
                .Where(combined);

            var tableQuerySegment = await _measurementsTable.ExecuteQuerySegmentedAsync(projectionQuery, null);

            var values = tableQuerySegment.Results.Select(me => new CounterDataValue
            {
                CounterId = me.CounterId,
                MeasureTime = me.MeasureTime,
                PulseValue = me.PulseValue
            }).ToList();

            return new QueryResult
            {
                Values = values
            };
        }
    }
}