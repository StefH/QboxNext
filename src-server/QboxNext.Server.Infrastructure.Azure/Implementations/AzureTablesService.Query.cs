﻿using Microsoft.Extensions.Logging;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Domain.Utils;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using QboxNext.Server.Infrastructure.Azure.Models.Internal;
using QboxNext.Server.Infrastructure.Azure.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsAzure.Table.Extensions;
using QboxNext.Server.Common.Utils;

namespace QboxNext.Server.Infrastructure.Azure.Implementations
{
    internal partial class AzureTablesService
    {
        /// <inheritdoc cref="IAzureTablesService.GetQboxRegistrationDetailsAsync(string)"/>
        public async Task<QboxRegistrationDetails> GetQboxRegistrationDetailsAsync(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
            {
                return null;
            }

            _logger.LogInformation("Querying Table {table} for SerialNumber {SerialNumber}", _registrationTable.Name, serialNumber);

            var result = await _registrationTable.Set.FirstOrDefaultAsync(r => r.SerialNumber == serialNumber);
            if (result == null)
            {
                return null;
            }

            return new QboxRegistrationDetails
            {
                FirmwareDownloadAllowed = result.FirmwareDownloadAllowed,
                Firmware = result.FirmwareVersion
            };
        }

        /// <inheritdoc cref="IAzureTablesService.QueryDataAsync(string, DateTime, DateTime, QboxQueryResolution, bool)"/>
        public async Task<QboxPagedDataQueryResult<QboxCounterData>> QueryDataAsync(string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, bool adjustHours)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            int adjustedHours = 0;
            DateTime fromQueryParameter = from;
            if (adjustHours)
            {
                fromQueryParameter = from.AddDays(-1);
                adjustedHours = TimeZoneUtils.GetHoursDifferenceFromUTC(from);
            }

            string fromPartitionKey = PartitionKeyHelper.Construct(serialNumber, fromQueryParameter);
            string toPartitionKey = PartitionKeyHelper.Construct(serialNumber, to);

            string fromRowKey = DateTimeRowKeyHelper.Construct(fromQueryParameter);
            string toRowKey = DateTimeRowKeyHelper.Construct(to);

            _logger.LogInformation("Querying Table {table} with PartitionKey {fromPartitionKey} to {toPartitionKey} and RowKey {fromRowKey} to {toRowKey} and AdjustHours = {adjustHours}", _measurementTable.Name, fromPartitionKey, toPartitionKey, fromRowKey, toRowKey, adjustHours);

            var queue = new ConcurrentQueue<List<MeasurementEntity>>();
            var tasks = EachDay(fromQueryParameter, to).Select(async date =>
            {
                var result = await _measurementTable.Set.Where(m =>
                    m.PartitionKey == PartitionKeyHelper.Construct(serialNumber, date) &&
                    string.CompareOrdinal(m.RowKey, fromRowKey) <= 0 && string.CompareOrdinal(m.RowKey, toRowKey) > 0
                ).ToListAsync();

                queue.Enqueue(result);
            });

            await Task.WhenAll(tasks);

            var entities = queue.SelectMany(x => x)
                .Where(e => e.MeasureTimeAdjusted != true) // Exclude adjusted measurements here. Not possible in real query above !
                .Where(e => !adjustHours || e.MeasureTime > from.AddHours(-adjustedHours)) // Filter
                .OrderBy(e => e.MeasureTime).ToList();

            // Adjust MeasureTime if needed
            if (adjustHours)
            {
                foreach (var entity in entities)
                {
                    entity.MeasureTime = entity.MeasureTime.AddHours(adjustedHours);
                }
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

            if (deltas.Count > 0)
            {
                deltas.Insert(0, new QboxCounterData
                {
                    MeasureTime = entities[0].MeasureTime,
                    Delta0181 = 0,
                    Delta0182 = 0,
                    Delta0281 = 0,
                    Delta0282 = 0,
                    Delta2421 = 0
                });
            }

            var groupedByTimeFrame = from delta in deltas
                                     group delta by new
                                     {
                                         MeasureTimeRounded = resolution.TruncateTime(delta.MeasureTime)
                                     }
                into g
                                     select new QboxCounterData
                                     {
                                         LabelText = resolution.GetLabelText(g.Key.MeasureTimeRounded),
                                         LabelValue = resolution.GetLabelValue(g.Key.MeasureTimeRounded),
                                         MeasureTime = g.Key.MeasureTimeRounded,
                                         Delta0181 = g.Sum(x => x.Delta0181),
                                         Delta0182 = g.Sum(x => x.Delta0182),
                                         Delta0281 = g.Sum(x => x.Delta0281) * -1,
                                         Delta0282 = g.Sum(x => x.Delta0282) * -1,
                                         Delta2421 = g.Sum(x => x.Delta2421)
                                     };

            var itemsFound = groupedByTimeFrame.ToList();

            var items = FillGaps(from, to, resolution, itemsFound);

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
            if (resolution > QboxQueryResolution.FiveMinutes)
            {
                foreach (var item in items)
                {
                    item.DrillDownQuery = GetDrillDownQboxDataQuery(item.MeasureTime, resolution, adjustHours);
                }
            }

            return new QboxPagedDataQueryResult<QboxCounterData>
            {
                Overview = overview,
                Items = items,
                Count = items.Count
            };
        }

        private static List<QboxCounterData> FillGaps(DateTime from, DateTime to, QboxQueryResolution resolution, List<QboxCounterData> itemsFound)
        {
            int steps = resolution.GetSteps(from, to);

            var items = new List<QboxCounterData>();
            for (int i = 0; i < steps; i++)
            {
                double delta = 1.0 * i / steps * (to - from).TotalMinutes;
                var measureTime = from.AddMinutes(delta);
                var measureTimeRounded = resolution.TruncateTime(measureTime);
                string labelText = resolution.GetLabelText(measureTimeRounded);

                var existing = itemsFound.FirstOrDefault(it => it.LabelText == labelText);
                if (existing != null)
                {
                    items.Add(existing);
                }
                else
                {
                    items.Add(new QboxCounterData
                    {
                        MeasureTime = measureTimeRounded,
                        LabelText = labelText,
                        LabelValue = resolution.GetLabelValue(measureTimeRounded)
                    });
                }
            }

            return items.OrderBy(e => e.MeasureTime).ToList();
        }

        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        private static QboxDataQuery GetDrillDownQboxDataQuery(DateTime measureTime, QboxQueryResolution resolution, bool adjustHours)
        {
            QboxQueryResolution resolutionNew = resolution - 1;
            var query = new QboxDataQuery
            {
                AdjustHours = adjustHours,
                Resolution = resolutionNew,
                From = resolution.TruncateTime(measureTime)
            };

            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
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