using QboxNext.Core.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
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
        public async Task<QboxPagedDataQueryResult<QboxCounterDataValue>> QueryDataAsync(string serialNumber, DateTime from, DateTime to, QboxQueryResolution resolution, int addHours)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            string fromPartitionKey = GetPartitionKey(serialNumber, from);
            string toPartitionKey = GetPartitionKey(serialNumber, to);
            bool same = fromPartitionKey == toPartitionKey;

            var entities = await _measurementTableSet
                .Where(m =>
                (
                    same && m.PartitionKey == fromPartitionKey ||
                    !same && string.CompareOrdinal(m.PartitionKey, fromPartitionKey) <= 0 && string.CompareOrdinal(m.PartitionKey, toPartitionKey) >= 0) &&
                    m.MeasureTime >= from && m.MeasureTime < to
                )
                .ToListAsync();

            if (entities.Count == 0)
            {
                return new QboxPagedDataQueryResult<QboxCounterDataValue>
                {
                    Extra = new QboxCounterDataValue(),
                    Items = new List<QboxCounterDataValue>(),
                    Count = 0
                };
            }

            var sorted = entities.OrderBy(e => e.MeasureTime).ToArray();

            //var timeslots = (from e in sorted
            //                 group e by new
            //                 {
            //                     MeasureTimeRounded = Get(e.MeasureTime, resolution)
            //                 }
            // into g
            //                 select new
            //                 {
            //                     LabelText = GetLabelText(g.Key.MeasureTimeRounded, resolution),
            //                     MeasureTime = g.Key.MeasureTimeRounded,
            //                 }).ToList();
            //var list = new List<QboxCounterDataValue>();

            //var first = sorted.First();
            //list.Add(new QboxCounterDataValue
            //{
            //    MeasureTime = first.MeasureTime,
            //    Delta0181 = first.Counter0181,
            //    Delta0182 = first.Counter0182,
            //    Delta0281 = first.Counter0281,
            //    Delta0282 = first.Counter0282,
            //    Delta2421 = first.Counter2421
            //});

            //foreach (var slot in timeslots.Skip(1))
            //for (int i = 1; i < timeslots.Count; i++)
            //{
            //    var entity = sorted.First(e => e.MeasureTime >= timeslots[i].MeasureTime);
            //    list.Add(new QboxCounterDataValue
            //    {
            //        MeasureTime = timeslots[i].MeasureTime,
            //        LabelText = timeslots[i].LabelText,
            //        Delta0181 = entity.Counter0181 - list[i - 1].Delta0181,
            //        Delta0182 = entity.Counter0182 - list[i - 1].Delta0182,
            //        Delta0281 = entity.Counter0281 - list[i - 1].Delta0281,
            //        Delta0282 = entity.Counter0282 - list[i - 1].Delta0282,
            //        Delta2421 = entity.Counter2421 - list[i - 1].Delta2421,
            //    });
            //}



            var deltas = sorted.Zip(sorted.Skip(1), (current, next) => new QboxCounterDataValue
            {
                MeasureTime = next.MeasureTime, // next.MeasureTime.Hour != 20 && next.MeasureTime.Hour != 6 ? next.MeasureTime.AddSeconds(-1) : next.MeasureTime, // new DateTime((long) new [] { next.MeasureTime.Ticks, current.MeasureTime.Ticks }.Average()),
                //Delta0181 = next.Counter0181.HasValue && current.Counter0181.HasValue ? next.Counter0181 - current.Counter0181 : null,
                //Delta0182 = next.Counter0182.HasValue && current.Counter0182.HasValue ? next.Counter0182 - current.Counter0182 : null,
                //Delta0281 = next.Counter0281.HasValue && current.Counter0281.HasValue ? next.Counter0281 - current.Counter0281 : null,
                //Delta0282 = next.Counter0282.HasValue && current.Counter0282.HasValue ? next.Counter0282 - current.Counter0282 : null,
                //Delta2421 = next.Counter2421.HasValue && current.Counter2421.HasValue ? next.Counter2421 - current.Counter2421 : null
                Delta0181 = next.Counter0181 - current.Counter0181,
                Delta0182 = next.Counter0182 - current.Counter0182,
                Delta0281 = next.Counter0281 - current.Counter0281,
                Delta0282 = next.Counter0282 - current.Counter0282,
                Delta2421 = next.Counter2421 - current.Counter2421
            }).ToList();

            //deltas.Add(new QboxCounterDataValue
            //{
            //    MeasureTime = sorted[0].MeasureTime,
            //    Delta0181 = sorted[1].Counter0181 - sorted[0].Counter0181,
            //    Delta0182 = sorted[1].Counter0182 - sorted[0].Counter0182,
            //    Delta0281 = sorted[1].Counter0281 - sorted[0].Counter0281,
            //    Delta0282 = sorted[1].Counter0282 - sorted[0].Counter0282,
            //    Delta2421 = sorted[1].Counter2421 - sorted[0].Counter2421
            //});

            deltas = deltas.OrderBy(d => d.MeasureTime).ToList();

            //foreach (var delta in deltas)
            //{
            //    // delta.MeasureTime = delta.MeasureTime.AddHours(addHours);
            //}

            //for (int i = 1; i < sorted.Length; i++)
            //{
            //    //if (sorted[i].MeasureTime.Hour != 20 && sorted[i].MeasureTime.Hour != 6) // Hack for Hoog-Laag
            //    //{
            //    //    sorted[i].MeasureTime = sorted[i].MeasureTime.AddSeconds(-1);
            //    //}
            //}

            var grouped = from e in deltas
                          group e by new
                          {
                              MeasureTimeRounded = Get(e.MeasureTime, resolution)
                          }
                into g
                          select new QboxCounterDataValue
                          {
                              LabelText = GetLabelText(g.Key.MeasureTimeRounded, resolution),
                              // LabelValue = GetLabelValue(g.Key.MeasureTimeRounded, resolution),
                              MeasureTime = g.Key.MeasureTimeRounded,
                              Delta0181 = g.Max(x => x.Delta0181) - g.Min(x => x.Delta0181), // !g.Max(x => x.Counter0181).HasValue || !g.Min(x => x.Counter0181).HasValue ? null : 
                              Delta0182 = g.Max(x => x.Delta0182) - g.Min(x => x.Delta0182), // !g.Max(x => x.Counter0182).HasValue || !g.Min(x => x.Counter0182).HasValue ? null : 
                              Delta0281 = (g.Max(x => x.Delta0281) - g.Min(x => x.Delta0281)) * -1, // !g.Max(x => x.Counter0281).HasValue || !g.Min(x => x.Counter0281).HasValue ? null : 
                              Delta0282 = (g.Max(x => x.Delta0282) - g.Min(x => x.Delta0282)) * -1, // !g.Max(x => x.Counter0282).HasValue || !g.Min(x => x.Counter0282).HasValue ? null : 
                              Delta2421 = g.Max(x => x.Delta2421) - g.Min(x => x.Delta2421) // !g.Max(x => x.Counter2421).HasValue || !g.Min(x => x.Counter2421).HasValue ? null : 
                          };

            var items = grouped.OrderBy(i => i.MeasureTime).ToList();

            var extra = new QboxCounterDataValue
            {
                LabelText = "extra",
                MeasureTime = DateTime.UtcNow,
                Delta0181 = entities.Max(e => e.Counter0181) - entities.Min(e => e.Counter0181),
                Delta0182 = entities.Max(e => e.Counter0182) - entities.Min(e => e.Counter0182),
                Delta0281 = (entities.Max(e => e.Counter0281) - entities.Min(e => e.Counter0281)) * -1,
                Delta0282 = (entities.Max(e => e.Counter0282) - entities.Min(e => e.Counter0282)) * -1,
                Delta2421 = entities.Max(e => e.Counter2421) - entities.Min(e => e.Counter2421)
            };

            return new QboxPagedDataQueryResult<QboxCounterDataValue>
            {
                Extra = extra,
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
                    return measureTime.Truncate(TimeSpan.FromHours(1));
                //return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Day:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, 0, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Month:
                    return new DateTime(measureTime.Year, measureTime.Month, 1, 0, 0, 0, measureTime.Kind);

                case QboxQueryResolution.Year:
                    return new DateTime(measureTime.Year, 1, 1, 0, 0, 0, measureTime.Kind);

                default:
                    throw new NotSupportedException();
            }
        }

        private static string GetLabelText(DateTime measureTime, QboxQueryResolution resolution)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    //if (measureTime.Hour != 20 && measureTime.Hour != 6)
                    //{
                    //    return measureTime.AddSeconds(1).ToString("HH:mm");
                    //}
                    //else
                    //{
                    //    return measureTime.ToString("HH:mm");
                    //}
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