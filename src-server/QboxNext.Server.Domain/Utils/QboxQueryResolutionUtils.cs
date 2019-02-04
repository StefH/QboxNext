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
    }
}