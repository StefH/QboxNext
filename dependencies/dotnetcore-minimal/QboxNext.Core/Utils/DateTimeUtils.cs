using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace QboxNext.Core.Utils
{
    public static class DateTimeUtils
    {
        /// <summary>
        /// Compute the maximum of a number of DateTime objects.
        /// </summary>
        public static DateTime MaxDateTime(params DateTime[] inDateTimes)
        {
            var max = inDateTimes[0];
            foreach (var datetime in inDateTimes)
            {
                if (datetime > max)
                    max = datetime;
            }
            return max;
        }


        /// <summary>
        /// Compute the minimum of a number of DateTime objects.
        /// </summary>
        public static DateTime MinDateTime(params DateTime[] inDateTimes)
        {
            var min = inDateTimes[0];
            foreach (var datetime in inDateTimes)
            {
                if (datetime < min)
                    min = datetime;
            }
            return min;
        }


        /// <summary>
        /// Parse a simple ISO 8601 date/time string, for example "2013-02-28T14:42".
        /// </summary>
        public static DateTime ParseIso8601DateTimeString(string inDateTimeString, DateTimeKind inDateTimeKind)
        {
            var regex = new Regex(@"(\d\d\d\d)-(\d\d)-(\d\d)(T(\d\d):(\d\d))?");
            var match = regex.Match(inDateTimeString);
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[3].Value);
            int hour = 0;
            int minute = 0;
            if (!String.IsNullOrEmpty(match.Groups[4].Value))
            {
                hour = int.Parse(match.Groups[5].Value);
                minute = int.Parse(match.Groups[6].Value);
            }

            return new DateTime(year, month, day, hour, minute, 0, inDateTimeKind);
        }


        /// <summary>
        /// UtcNow is updated every 10-15 msec
        /// http://stackoverflow.com/questions/5608980/how-to-ensure-a-timestamp-is-always-unique/14369695#14369695
        /// </summary>
        public static DateTime HiResUtcNow
        {
            get
            {
                long original, newValue;
                do
                {
                    original = _lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    // increment with 100.000 (10msec) to prevent sql server datetime accuracy issues 
                    // http://msdn.microsoft.com/en-us/library/ms187819.aspx 
                    // http://msdn.microsoft.com/en-us/library/vstudio/system.datetime.ticks(v=vs.100).aspx
                    newValue = Math.Max(now, original + 100000);
                } while (Interlocked.CompareExchange
                                (ref _lastTimeStamp, newValue, original) != original);

                return new DateTime(newValue);
            }
        }


        /// <summary>
        /// UtcNow is updated every 10-15 msec
        /// http://stackoverflow.com/questions/5608980/how-to-ensure-a-timestamp-is-always-unique/14369695#14369695
        /// </summary>
        public static DateTime HiResUtcNowForLog
        {
            get
            {
                long original, newValue;
                do
                {
                    original = _lastTimeStamp;
                    long now = DateTime.UtcNow.Ticks;
                    newValue = Math.Max(now, original + 1);
                } while (Interlocked.CompareExchange
                                (ref _lastTimeStamp, newValue, original) != original);

                return new DateTime(newValue);
            }
        }


        public static int GetYears(DateTime firstDate, DateTime secondDate)
        {
            int years = secondDate.Year - firstDate.Year;
            // Check on full year, ie december 2013 -> april 2014 is NOT 1 year
            if (firstDate > secondDate.AddYears(-years)) years--;
            return years;
        }


        public static int GetAge(DateTime inBirthday)
        {
            return GetYears(inBirthday, DateTime.Today);
        }


        public static DateTime NlDateToUtc(DateTime inDateNl)
        {
            Debug.Assert(inDateNl.Kind == DateTimeKind.Local || inDateNl.Kind == DateTimeKind.Unspecified);
            return NlDateToUtc(inDateNl.Year, inDateNl.Month, inDateNl.Day);
        }


        public static DateTime NlDateToUtc(int inYear, int inMonth, int inDay)
        {
            var nlDateTime = new DateTime(inYear, inMonth, inDay, 0, 0, 0, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTime(nlDateTime, TimeZoneNl, TimeZoneInfo.Utc);
        }


        public static DateTime NlDateTimeToUtc(DateTime inDateTimeNl)
        {
            if (inDateTimeNl.Kind == DateTimeKind.Utc)
                return inDateTimeNl;

            if (inDateTimeNl.Kind == DateTimeKind.Local)
            {
                // This probably means that a UTC time was passed to a controller and MVC converted it to local time. We can safely convert it back to UTC.
                return inDateTimeNl.ToUniversalTime();
            }

            return TimeZoneInfo.ConvertTimeToUtc(inDateTimeNl, TimeZoneNl);
        }


        public static DateTime UtcDateTimeToNl(DateTime inDateTimeUtc)
        {
            Debug.Assert(inDateTimeUtc.Kind == DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTime(inDateTimeUtc, TimeZoneInfo.Utc, TimeZoneNl);
        }


        /// <summary>
        /// Assume that inDateTime was meant as UTC.
        /// </summary>
        public static DateTime AssumeUtc(DateTime inDateTime)
        {
            if (inDateTime.Kind == DateTimeKind.Unspecified)
                return DateTime.SpecifyKind(inDateTime, DateTimeKind.Utc);

            if (inDateTime.Kind == DateTimeKind.Local)
            {
                // Probably local time converted from UTC.
                return inDateTime.ToUniversalTime();
            }

            return inDateTime;
        }


        public static int Quarter(this DateTime inDateTime)
        {
            return ((inDateTime.Month - 1) / 3) + 1;
        }


        /// <summary>
        /// result = [year][quarter] for sorting
        /// </summary>
        public static int YearQuarter(this DateTime inDateTime)
        {
            return inDateTime.Year * 10 + inDateTime.Quarter();
        }


        public static DateTime FirstDayOfQuarter(this DateTime inDateTime)
        {
            return new DateTime(inDateTime.Year, inDateTime.Quarter() * 3 - 2, 1);
        }


        public static DateTime LastDayOfQuarter(this DateTime inDateTime)
        {
            return inDateTime.FirstDayOfQuarter().AddMonths(3).AddDays(-1);
        }


        public static int DaysInQuarter(this DateTime inDateTime)
        {
            return (inDateTime.LastDayOfQuarter() - inDateTime.FirstDayOfQuarter()).Days + 1;
        }


        public static long UnixTimeStampNow()
        {
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static bool InSameMonth(this DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }

        public static readonly TimeZoneInfo TimeZoneNl = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
        private static long _lastTimeStamp = DateTime.UtcNow.Ticks;
    }
}
