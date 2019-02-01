using QboxNext.Core.Extensions;
using QboxNext.Server.Common.Validation;
using QboxNext.Server.Domain;
using QboxNext.Server.Infrastructure.Azure.Interfaces.Public;
using System;
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
                    m.MeasureTime >= from && m.MeasureTime <= to
                )
                .ToListAsync();


            var sorted = entities.OrderBy(e => e.MeasureTime).ToArray();
            for (int i = 1; i < sorted.Length; i++)
            {
                if (sorted[i].MeasureTime.Hour != 20 && sorted[i].MeasureTime.Hour != 6) // Hack for Hoog-Laag
                {
                    sorted[i].MeasureTime = sorted[i].MeasureTime.AddSeconds(-1);
                }
            }

            var grouped = from e in entities
                          group e by new
                          {
                              MeasureTimeRounded = Get(e.MeasureTime.AddHours(addHours), resolution)
                          }
                into g
                          select new QboxCounterDataValue
                          {
                              LabelText = GetLabelText(g.Key.MeasureTimeRounded, resolution),
                              LabelValue = GetLabelValue(g.Key.MeasureTimeRounded, resolution),
                              MeasureTime = g.Key.MeasureTimeRounded,
                              Delta0181 = g.Max(x => x.Counter0181) - g.Min(x => x.Counter0181), // !g.Max(x => x.Counter0181).HasValue || !g.Min(x => x.Counter0181).HasValue ? null : 
                              Delta0182 = g.Max(x => x.Counter0182) - g.Min(x => x.Counter0182), // !g.Max(x => x.Counter0182).HasValue || !g.Min(x => x.Counter0182).HasValue ? null : 
                              Delta0281 = (g.Max(x => x.Counter0281) - g.Min(x => x.Counter0281)) * -1, // !g.Max(x => x.Counter0281).HasValue || !g.Min(x => x.Counter0281).HasValue ? null : 
                              Delta0282 = (g.Max(x => x.Counter0282) - g.Min(x => x.Counter0282)) * -1, // !g.Max(x => x.Counter0282).HasValue || !g.Min(x => x.Counter0282).HasValue ? null : 
                              Delta2421 = g.Max(x => x.Counter2421) - g.Min(x => x.Counter2421) // !g.Max(x => x.Counter2421).HasValue || !g.Min(x => x.Counter2421).HasValue ? null : 
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
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, 0, 0, measureTime.Kind);

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
                    if (measureTime.Hour != 20 && measureTime.Hour != 6)
                    {
                        return measureTime.AddSeconds(1).ToString("HH:mm");
                    }
                    else
                    {
                        return measureTime.ToString("HH:mm");
                    }

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