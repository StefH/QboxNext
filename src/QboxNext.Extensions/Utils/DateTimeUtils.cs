using System;

namespace QboxNext.Extensions.Utils
{
    public static class DateTimeUtils
    {
        private static readonly TimeZoneInfo AmsterdamTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        public static DateTime ToAmsterdam(this DateTime input)
        {
            return TimeZoneInfo.ConvertTime(input, AmsterdamTimeZoneInfo);
        }
    }
}