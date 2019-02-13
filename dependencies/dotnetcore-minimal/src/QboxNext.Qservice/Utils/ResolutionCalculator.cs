using System;
using QboxNext.Core.Utils;

namespace QboxNext.Qservice.Utils
{
    /// <summary>
    /// ResolutionCalculator determines the resolution to use for different timespans.
    /// </summary>
    public class ResolutionCalculator
    {
        public SeriesResolution Calculate(DateTime from, DateTime to)
        {
            var span = to - from;
            var firstDayToInclude = from;
            var lastDayToInclude = to.AddDays(-1);

            if (span.TotalMinutes <= (int)SeriesResolution.Hour)
            {
                // For a very short period (for example when retrieving the last five minutes to determine live usage), use minute resolution.
                return SeriesResolution.OneMinute;
            }
            if (span.TotalMinutes <= (int)SeriesResolution.Day)
            {
                // For day graph, use resolution of five minutes.
                return SeriesResolution.FiveMinutes;
            }
            if (IsMostLikelyYearGraphRequestOrCustomPeriodTotalRequest(firstDayToInclude, lastDayToInclude) && !IsPossiblyWeekGraph(span))
            {
                // For year graph, use resolution of months.
                return SeriesResolution.Month;
            }
            // For other graphs (for example week graph), use resolution of days.
            return SeriesResolution.Day;
        }

        private static bool IsMostLikelyYearGraphRequestOrCustomPeriodTotalRequest(DateTime firstDayToInclude, DateTime lastDayToInclude)
        {
            return ((firstDayToInclude.Month != lastDayToInclude.Month) || firstDayToInclude.Year != lastDayToInclude.Year);
        }

        private static bool IsPossiblyWeekGraph(TimeSpan span)
        {
            return Math.Abs(span.TotalMinutes - (int)SeriesResolution.Week) <= 1;
        }
    }
}
