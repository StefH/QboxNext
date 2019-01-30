using QboxNext.Core.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
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

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, DateTime, DateTime, QboxQueryResolution)"/>
        public async Task<PagedQueryResult<QboxCounterDataValue>> QueryDataAsync(string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            string fromPartitionKey = GetPartitionKey(serialNumber, from);
            string toPartitionKey = GetPartitionKey(serialNumber, to);
            bool same = fromPartitionKey == toPartitionKey;

            var entities = await _measurementTableSet
                .Where(m => (same && m.PartitionKey == fromPartitionKey || !same && string.CompareOrdinal(m.PartitionKey, fromPartitionKey) <= 0 && string.CompareOrdinal(m.PartitionKey, toPartitionKey) >= 0) &&
                    m.MeasureTime >= from && m.MeasureTime < to
                )
                .ToListAsync();

            var grouped = from v in entities
                          group v by new
                          {
                              MeasureTimeRounded = Get(v.MeasureTime, resolution)
                          }
                into g
                          select new QboxCounterDataValue
                          {
                              MeasureTime = g.Key.MeasureTimeRounded,
                              Label = GetLabel(g.Key.MeasureTimeRounded, resolution),
                              Delta0181 = !g.Max(x => x.Counter0181).HasValue || !g.Min(x => x.Counter0181).HasValue ? null : g.Max(x => x.Counter0181) - g.Min(x => x.Counter0181),
                              Delta0182 = !g.Max(x => x.Counter0182).HasValue || !g.Min(x => x.Counter0182).HasValue ? null : g.Max(x => x.Counter0182) - g.Min(x => x.Counter0182),
                              Delta0281 = !g.Max(x => x.Counter0281).HasValue || !g.Min(x => x.Counter0281).HasValue ? null : (g.Max(x => x.Counter0281) - g.Min(x => x.Counter0281)) * -1,
                              Delta0282 = !g.Max(x => x.Counter0282).HasValue || !g.Min(x => x.Counter0282).HasValue ? null : (g.Max(x => x.Counter0282) - g.Min(x => x.Counter0282)) * -1,
                              Delta2421 = !g.Max(x => x.Counter2421).HasValue || !g.Min(x => x.Counter2421).HasValue ? null : g.Max(x => x.Counter2421) - g.Min(x => x.Counter2421)
                          };

            var items = grouped.OrderBy(e => e.MeasureTime).ToList();

            return new PagedQueryResult<QboxCounterDataValue>
            {
                Items = items,
                Count = items.Count
            };
        }

        private static DateTime Get(DateTime measureTime, QboxQueryResolution resolution)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    return measureTime.Truncate(TimeSpan.FromMinutes(15));

                case QboxQueryResolution.Hour:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Day:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, 0, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Week:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day / 7 * 7, 0, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Month:
                    return new DateTime(measureTime.Year, measureTime.Month, 0, 0, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Year:
                    return new DateTime(measureTime.Year, 0, 0, 0, 0, 0, measureTime.Kind);

                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetLabel(DateTime measureTime, QboxQueryResolution resolution)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    return measureTime.ToString("HH:mm");

                case QboxQueryResolution.Hour:
                    return measureTime.ToString("HH");

                case QboxQueryResolution.Day:
                    return measureTime.ToString("dd");

                case QboxQueryResolution.Week:
                    return $"{measureTime.Day / 7}";

                case QboxQueryResolution.Month:
                    return measureTime.ToString("MM");

                case QboxQueryResolution.Year:
                    return measureTime.ToString("yyyy");

                default:
                    throw new NotSupportedException();
            }
        }
    }
}