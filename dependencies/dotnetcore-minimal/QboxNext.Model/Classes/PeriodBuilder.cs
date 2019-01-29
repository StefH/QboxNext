using System;
using QboxNext.Core.Extensions;

namespace QboxNext.Model.Classes
{
    public class Period
    {
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }

        public Period(DateTime from, DateTime to)
        {
            From = @from;
            To = to;
        }

        public TimeSpan Span
        {
            get
            {
                return To - From;
            }
        }

        public Period AddDays(int days)
        {
            return new Period(From.AddDays(days), To.AddDays(days));
        }

        public Period RelativeDays(Period source)
        {
            var days = (int)source.Span.TotalDays * -1;
            return AddDays(days);
        }
    }

    public class PeriodBuilder
    {
        public static Period Today()
        {
            return new Period(DateTime.Today, DateTime.Now);
        }

        public static Period Yesterday()
        {
            return new Period(DateTime.Today.AddDays(-1), DateTime.Today);
        }

        public static Period ThisWeek()
        {
            return new Period(DateTime.Today.FirstDayOfWeek(), DateTime.Now);
        }

        public static Period LastWeek()
        {
            var from = DateTime.Today.AddDays(-7).FirstDayOfWeek();
            var to = from.AddDays(7);
            return new Period(from, to);
        }

        public static Period ThisMonth()
        {
            return new Period(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), DateTime.Now);
        }

        public static Period LastMonth()
        {
            //todo: Als deze maand minder dagen heeft dan vorige maand dan eerste dagen niet meetellen (#1607)
            // Dit geeft een vreemd resultaat als je de vorige maand in tijd laat veranderen
            // en strookt niet met de algemene opvatting... Rolf issue: #
            var to = DateTime.Today.FirstDayOfMonth();
            var thisMonthDays = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
            var lastMonthDays = DateTime.DaysInMonth(DateTime.Today.AddMonths(-1).Year, DateTime.Today.AddMonths(-1).Month);
            var delta = thisMonthDays < lastMonthDays ? thisMonthDays : lastMonthDays;
            var result = new Period(to.AddDays(delta * -1), to);
           
            return result;
        }

        public static Period ThisYear()
        {
            return new Period(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Now);
        }

        public static Period LastYear()
        {
            return new Period(new DateTime(DateTime.Today.Year - 1, 1, 1), new DateTime(DateTime.Today.Year, 1, 1));
        }

        public static Period YesterdayRelative()
        {
            return new Period(DateTime.Today.AddDays(-1), DateTime.Now.AddDays(-1));
        }

        public static Period ThisWeekRelative()
        {
            return new Period(DateTime.Today.FirstDayOfWeek(), DateTime.Now);
        }

        public static Period LastWeekRelative()
        {
            return new Period ( DateTime.Today.FirstDayOfWeek().AddDays(-7), DateTime.Now.AddDays(-7));
        }

        public static Period ThisMonthRelative()
        {
            return new Period(DateTime.Today.FirstDayOfMonth(), DateTime.Now);
        }

        public static Period LastMonthRelative()
        {
            var to = DateTime.Now.AddMonths(-1);
            var from = to.FirstDayOfMonth();

            // qplat-5: Als huidige dag groter is dan aantal dagen van de vorige maand, dan wordt de eerste dag van deze maand als einddatum gebruikt
            if (to.Day < DateTime.Now.Day)
                to = DateTime.Now.FirstDayOfMonth();

            return new Period(from, to);
        }

        public static Period ThisYearRelative()
        {
            return new Period(new DateTime(DateTime.Today.Year, 1, 1), DateTime.Now);
        }

        public static Period LastYearRelative()
        {
            var thisYear = ThisYearRelative();
            return new Period(thisYear.From.AddYears(-1), thisYear.To.AddYears(-1));
        }
    }
}
