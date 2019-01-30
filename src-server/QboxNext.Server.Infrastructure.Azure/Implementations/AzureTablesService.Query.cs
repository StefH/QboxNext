using QboxNext.Core.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
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

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, IList{int}, DateTime, DateTime, QboxQueryResolution)"/>
        public async Task<PagedQueryResult<QboxCounterDataValue>> QueryDataAsync(string serialNumber, IList<int> counterIds, DateTime from, DateTime to, QboxQueryResolution resolution)
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

            entities = entities.OrderBy(e => e.MeasureTime).ToList();

            // var xxx = entities.Where(e => e.MeasureTime >= new DateTime(2018, 5, 1, 4, 0, 0) && e.MeasureTime < new DateTime(2018, 5, 1, 5, 1, 1)).ToList();

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
                              Delta2421 = !g.Max(x => x.Counter2421).HasValue || !g.Min(x => x.Counter2421).HasValue ? null : g.Max(x => x.Counter2421) - g.Min(x => x.Counter2421 )
                          };

            //var items0181 = entities.Where(e => counterIds.Contains(181) && e.Counter0181 != null).Select(e => new { CounterId = 181, PulseValue = e.Counter0181.Value, e.MeasureTime });
            //var items0182 = entities.Where(e => counterIds.Contains(182) && e.Counter0182 != null).Select(e => new { CounterId = 182, PulseValue = e.Counter0182.Value, e.MeasureTime });
            //var items0281 = entities.Where(e => counterIds.Contains(281) && e.Counter0281 != null).Select(e => new { CounterId = 281, PulseValue = e.Counter0281.Value, e.MeasureTime });
            //var items0282 = entities.Where(e => counterIds.Contains(282) && e.Counter0282 != null).Select(e => new { CounterId = 282, PulseValue = e.Counter0282.Value, e.MeasureTime });
            //var items2421 = entities.Where(e => counterIds.Contains(2421) && e.Counter2421 != null).Select(e => new { CounterId = 2421, PulseValue = e.Counter2421.Value, e.MeasureTime });

            //var values = items0181.Union(items0182).Union(items0281).Union(items0282).Union(items2421).ToList();

            //var grouped = from v in values
            //              group v by new
            //              {
            //                  v.CounterId,
            //                  MeasureTimeRounded = Get(v.MeasureTime, resolution)
            //              }
            //    into g
            //              select new QboxCounterDataValue
            //              {
            //                  CounterId = g.Key.CounterId,
            //                  MeasureTime = g.Key.MeasureTimeRounded,
            //                  Label = GetLabel(g.Key.MeasureTimeRounded, resolution),
            //                  AveragePulseValue = (int)g.Average(c => c.PulseValue),
            //                  Min = g.Min(c => c.PulseValue),
            //                  Max = g.Max(c => c.PulseValue),
            //                  Delta = g.Max(c => c.PulseValue) - g.Min(c => c.PulseValue)
            //              };

            var items = grouped.ToList();

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