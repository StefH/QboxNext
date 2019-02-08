using System;

namespace QboxNext.Core.Extensions
{
    /// <summary>
    /// Helper class om DateTime waarden af te ronden of te truncaten, voorbeelden:
    /// new DateTime(2010, 11, 4, 10, 28, 27).Round(TimeSpan.FromMinutes(1)); // rounds to 2010.11.04 10:28:00
    /// new DateTime(2010, 11, 4, 13, 28, 27).Round(TimeSpan.FromDays(1)); // rounds to 2010.11.05 00:00
    /// </summary>
    public static class DateTimeTruncateExtensions
    {
        public static TimeSpan Truncate(this TimeSpan time, TimeSpan roundingInterval)
        {
            return new TimeSpan(
                Convert.ToInt64(Math.Truncate(
                    time.Ticks / (decimal)roundingInterval.Ticks
                )) * roundingInterval.Ticks
            );
        }

        public static DateTime Truncate(this DateTime datetime, TimeSpan roundingInterval)
        {
            return new DateTime((datetime - DateTime.MinValue).Truncate(roundingInterval).Ticks, datetime.Kind);
        }


        public static DateTime TruncateToMinute(this DateTime inDateTime)
        {
            return new DateTime(inDateTime.Year, inDateTime.Month, inDateTime.Day, inDateTime.Hour, inDateTime.Minute, 0, inDateTime.Kind);
        }
    }
}
