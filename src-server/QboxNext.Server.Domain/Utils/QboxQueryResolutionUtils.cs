using QboxNext.Core.Extensions;
using System;
using System.Globalization;

namespace QboxNext.Server.Domain.Utils
{
    public static class QboxQueryResolutionUtils
    {
        public static int GetSteps(this QboxQueryResolution resolution, DateTime from, DateTime to)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    return 24 * 4;

                case QboxQueryResolution.Hour:
                    return 24;

                case QboxQueryResolution.Day:
                    return (to - from).Days;

                case QboxQueryResolution.Month:
                    return to.Month - from.Month;

                case QboxQueryResolution.Year:
                    return to.Year - from.Year;

                default:
                    throw new NotSupportedException();
            }
        }

        public static string GetLabelText(this QboxQueryResolution resolution, DateTime measureTime)
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

        public static int GetLabelValue(this QboxQueryResolution resolution, DateTime measureTime)
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

        public static DateTime TruncateTime(this QboxQueryResolution resolution, DateTime measureTime)
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

        public static (DateTime start, DateTime end) GetTruncatedTimeFrame(this QboxQueryResolution resolution, DateTime from, DateTime to)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                case QboxQueryResolution.Hour:
                case QboxQueryResolution.Day:
                    return (new DateTime(from.Year, from.Month, from.Day, 0, 0, 0, from.Kind), new DateTime(to.Year, to.Month, to.Day, 0, 0, 0, to.Kind));

                case QboxQueryResolution.Month:
                case QboxQueryResolution.Year:
                    return (new DateTime(from.Year, from.Month, 1, 0, 0, 0, from.Kind), new DateTime(to.Year, to.Month, 1, 0, 0, 0, to.Kind));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}