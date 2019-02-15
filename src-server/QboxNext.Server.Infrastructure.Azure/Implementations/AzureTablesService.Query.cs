using Microsoft.Extensions.Logging;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Domain.Utils;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Utils;
using System;
using System.Collections.Concurrent;
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

            _logger.LogInformation("Querying Table {table} for SerialNumber {SerialNumber}", _registrationTable.Name, serialNumber);

            return await _registrationTable.Set.FirstOrDefaultAsync(r => r.SerialNumber == serialNumber) != null;
        }

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, DateTime, DateTime, QboxQueryResolution, int)"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> QueryDataAsync(string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, int addHours)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            string fromPartitionKey = PartitionKeyHelper.ConstructPartitionKey(serialNumber, from);
            string toPartitionKey = PartitionKeyHelper.ConstructPartitionKey(serialNumber, to);

            string fromRowKey = RowKeyHelper.GetRowKey(from);
            string toRowKey = RowKeyHelper.GetRowKey(to);

            _logger.LogInformation("Querying Table {table} with PartitionKey {fromPartitionKey} to {toPartitionKey} and RowKey {fromRowKey} to {toRowKey}", _measurementTable.Name, fromPartitionKey, toPartitionKey, fromRowKey, toRowKey);

            var queue = new ConcurrentQueue<List<MeasurementEntity>>();
            var tasks = EachDay(from, to).Select(async date =>
            {
                var result = await _measurementTable.Set.Where(
                    m => m.PartitionKey == PartitionKeyHelper.ConstructPartitionKey(serialNumber, date) &&
                    string.CompareOrdinal(m.RowKey, fromRowKey) <= 0 && string.CompareOrdinal(m.RowKey, toRowKey) > 0
                ).ToListAsync();

                queue.Enqueue(result);
            });

            await Task.WhenAll(tasks);

            var entities = queue.SelectMany(x => x).OrderBy(e => e.MeasureTime).ToList();
            if (entities.Count == 0)
            {
                return new QboxPagedDataQueryResult<QboxCounterData>
                {
                    Overview = new QboxCounterData(),
                    Items = new List<QboxCounterData>(),
                    Count = 0
                };
            }

            var deltas = entities.Zip(entities.Skip(1), (current, next) => new QboxCounterData
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
                MeasureTime = entities[0].MeasureTime,
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

            // Define DrillDownQuery
            if (resolution > QboxQueryResolution.QuarterOfHour)
            {
                foreach (var item in items)
                {
                    item.DrillDownQuery = GetDrillDownQboxDataQuery(item.MeasureTime, resolution, addHours);
                }
            }

            return new QboxPagedDataQueryResult<QboxCounterData>
            {
                Overview = overview,
                Items = items,
                Count = items.Count
            };
        }

        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
            {
                yield return day;
            }
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

        private static QboxDataQuery GetDrillDownQboxDataQuery(DateTime measureTime, QboxQueryResolution resolution, int addHours)
        {
            QboxQueryResolution resolutionNew = resolution - 1;
            var query = new QboxDataQuery
            {
                AddHours = addHours,
                Resolution = resolutionNew,
                From = resolution.TruncateTime(measureTime)
            };

            switch (resolution)
            {
                case QboxQueryResolution.Hour:
                case QboxQueryResolution.Day:
                    query.To = query.From.AddDays(1);
                    break;

                case QboxQueryResolution.Month:
                    query.To = query.From.AddMonths(1);
                    break;

                case QboxQueryResolution.Year:
                    query.To = query.From.AddYears(1);
                    break;

                default:
                    throw new NotSupportedException();
            }

            return query;
        }
    }
}