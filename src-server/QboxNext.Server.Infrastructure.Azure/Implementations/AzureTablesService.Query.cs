using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsAzure.Table.Extensions;

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

            return await _registrationTableSet.FirstOrDefaultAsync(r => r.SerialNumber == serialNumber) != null;
        }

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, IList{int}, DateTime, DateTime, QueryResolution)"/>
        public async Task<PagedQueryResult<CounterDataValue>> QueryDataAsync(string serialNumber, IList<int> counterIds, DateTime from, DateTime to, QueryResolution resolution)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            var entities = await _measurementTableSet
                .Where(m =>
                    m.SerialNumber == serialNumber &&
                    counterIds.Contains(m.CounterId) &&
                    m.MeasureTime >= from.ToUniversalTime() &&
                    m.MeasureTime < to.ToUniversalTime()
                ).
                Select(entity => new CounterDataValue
                {
                    CounterId = entity.CounterId,
                    MeasureTime = entity.MeasureTime,
                    PulseValue = entity.PulseValue
                })
                .ToListAsync();

            var grouped = from entity in entities
                          group entity by new
                          {
                              entity.CounterId,
                              MeasureTimeRounded = Get(entity.MeasureTime, resolution)
                          }
                into g
                          select new CounterDataValue
                          {
                              CounterId = g.Key.CounterId,
                              MeasureTime = g.Key.MeasureTimeRounded,
                              PulseValue = g.Max(s => s.PulseValue)
                          };

            var sorted = grouped.OrderBy(cv => cv.MeasureTime).ThenBy(cv => cv.CounterId).ToList();

            return new PagedQueryResult<CounterDataValue>
            {
                Items = sorted,
                Count = sorted.Count
            };
        }

        private DateTime Get(DateTime measureTime, QueryResolution resolution)
        {
            switch (resolution)
            {
                case QueryResolution.Hour:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}