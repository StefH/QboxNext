using System;

namespace QboxNext.Server.Domain.Utils
{
    public static class QboxQueryResolutionUtils
    {
        public static DateTime TruncateTime(this QboxQueryResolution resolution, DateTime measureTime)
        {
            switch (resolution)
            {
                case QboxQueryResolution.QuarterOfHour:
                    return new DateTime(measureTime.Year, measureTime.Month, measureTime.Day, measureTime.Hour, measureTime.Minute / 15 * 15, 0, measureTime.Kind);

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