using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Domain.Utils;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, DateTime, DateTime, QboxQueryResolution, int)"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> QueryDataAsync(string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, int addHours)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            string fromPartitionKey = GetPartitionKey(serialNumber, from);
            string toPartitionKey = GetPartitionKey(serialNumber, to);
            bool same = fromPartitionKey == toPartitionKey;

            string fromRowKey = GetRowKey(from);
            string toRowKey = GetRowKey(to);

            var entities = await _measurementTableSet
                .Where(m =>
                (
                    same && m.PartitionKey == fromPartitionKey ||
                    !same && string.CompareOrdinal(m.PartitionKey, fromPartitionKey) <= 0 && string.CompareOrdinal(m.PartitionKey, toPartitionKey) > 0) &&
                    string.CompareOrdinal(m.RowKey, fromRowKey) <= 0 && string.CompareOrdinal(m.RowKey, toRowKey) > 0
                )
                .ToListAsync();

            if (entities.Count == 0)
            {
                return new QboxPagedDataQueryResult<QboxCounterData>
                {
                    Overview = new QboxCounterData(),
                    Items = new List<QboxCounterData>(),
                    Count = 0
                };
            }

            var entitiesSorted = entities.OrderBy(e => e.MeasureTime).ToList();

            var deltas = entitiesSorted.Zip(entitiesSorted.Skip(1), (current, next) => new QboxCounterData
            {
                MeasureTime = next.MeasureTime,
                Delta0181 = next.Counter0181 - current.Counter0181 ?? 0,
                Delta0182 = next.Counter0182 - current.Counter0182 ?? 0,
                Delta0281 = next.Counter0281 - current.Counter0281 ?? 0,
                Delta0282 = next.Counter0282 - current.Counter0282 ?? 0,
                Delta2421 = next.Counter2421 - current.Counter2421 ?? 0,
            }).ToList();

            deltas.Insert(0, new QboxCounterData
            {
                MeasureTime = entitiesSorted[0].MeasureTime,
                Delta0181 = 0,
                Delta0182 = 0,
                Delta0281 = 0,
                Delta0282 = 0,
                Delta2421 = 0
            });

            var groupedByTimeFrame = from delta in deltas
                                     group delta by new
                                     {
                                         MeasureTimeRounded = resolution.TruncateTime(delta.MeasureTime)
                                     }
                into g
                                     select new QboxCounterData
                                     {
                                         LabelText = GetLabelText(g.Key.MeasureTimeRounded, resolution),
                                         LabelValue = GetLabelValue(g.Key.MeasureTimeRounded, resolution),
                                         MeasureTime = g.Key.MeasureTimeRounded,
                                         Delta0181 = g.Sum(x => x.Delta0181),
                                         Delta0182 = g.Sum(x => x.Delta0182),
                                         Delta0281 = g.Sum(x => x.Delta0281) * -1,
                                         Delta0282 = g.Sum(x => x.Delta0282) * -1,
                                         Delta2421 = g.Sum(x => x.Delta2421)
                                     };

            var items = groupedByTimeFrame.OrderBy(i => i.MeasureTime).ToList();

            var overview = new QboxCounterData
            {
                LabelText = "overview",
                MeasureTime = DateTime.UtcNow,
                Delta0181 = entities.Max(e => e.Counter0181) - entities.Min(e => e.Counter0181) ?? 0,
                Delta0182 = entities.Max(e => e.Counter0182) - entities.Min(e => e.Counter0182) ?? 0,
                Delta0281 = (entities.Max(e => e.Counter0281) - entities.Min(e => e.Counter0281) ?? 0) * -1,
                Delta0282 = (entities.Max(e => e.Counter0282) - entities.Min(e => e.Counter0282) ?? 0) * -1,
                Delta2421 = entities.Max(e => e.Counter2421) - entities.Min(e => e.Counter2421) ?? 0
            };

            return new QboxPagedDataQueryResult<QboxCounterData>
            {
                Overview = overview,
                Items = items,
                Count = items.Count
            };
        }

        private static string GetLabelText(DateTime measureTime, QboxQueryResolution resolution)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    return measureTime.ToString("HH:mm");

                case QboxQueryResolution.Hour:
                    return measureTime.ToString("HH u");

                case QboxQueryResolution.Day:
                    return measureTime.Day.ToString();

                case QboxQueryResolution.Month:
                    return measureTime.ToString("MMM", new CultureInfo("nl-NL"));

                case QboxQueryResolution.Year:
                    return measureTime.ToString("yyyy");

                default:
                    throw new NotSupportedException();
            }
        }

        private static int GetLabelValue(DateTime measureTime, QboxQueryResolution resolution)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                case QboxQueryResolution.Hour:
                    return measureTime.Hour;

                case QboxQueryResolution.Day:
                    return measureTime.Day;

                case QboxQueryResolution.Month:
                    return measureTime.Month;

                case QboxQueryResolution.Year:
                    return measureTime.Year;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}