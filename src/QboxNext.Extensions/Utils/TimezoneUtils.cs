using System;

namespace QboxNext.Extensions.Utils
{
    public static class TimeZoneUtils
    {
        private static readonly TimeZoneInfo UTC = TimeZoneInfo.FindSystemTimeZoneById("UTC");
        private static readonly TimeZoneInfo Amsterdam = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        /// <summary>
        /// Gets the hours difference between Amsterdam and UTC calculated for the time at that moment.
        /// </summary>
        /// <param name="moment">The moment.</param>
        public static int GetHoursDifferenceFromUTC(DateTime moment)
        {
            var difference = Amsterdam.GetUtcOffset(moment) - UTC.GetUtcOffset(moment);
            return difference.Hours;
        }
    }
}