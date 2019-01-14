using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QboxNext.Qserver.Core.Extensions
{
    public static class DateExtensions
    {
        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month + 1, 1);
        }

        public static DateTime FirstDayOfWeek(this DateTime date)
        {
            var delta = DayOfWeek.Monday - date.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            return date.AddDays(delta);
        }

        public static DateTime LastDayOfWeek(this DateTime date)
        {
            return date.FirstDayOfWeek().AddDays(7);
        }
    }
}
