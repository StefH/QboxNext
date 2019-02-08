using System;

namespace QboxNext.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }

        public static DateTime FirstDayOfWeek(this DateTime date)
        {
            int delta = DayOfWeek.Monday - date.DayOfWeek;
            if (delta > 0)
            {
                delta -= 7;
            }

            return date.Date.AddDays(delta);
        }

        public static DateTime LastDayOfWeek(this DateTime date)
        {
            return date.FirstDayOfWeek().AddDays(6);
        }
    }
}