using System;
using NUnit.Framework;
using QboxNext.Qserver.Core.Extensions;

namespace QboxNext.Model.Classes
{
    /// <summary>
    ///This is a test class for PeriodBuilderTest and is intended
    ///to contain all PeriodBuilderTest Unit Tests
    ///</summary>
    [TestFixture]
    public class PeriodBuilderTest
    {
        /// <summary>
        ///A test for ThisMonth
        ///</summary>
        [Test]
        public void ThisMonthTest()
        {
            var expected = DateTime.Now - new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var actual = PeriodBuilder.ThisMonth().Span;
            Assert.AreEqual((int)expected.TotalMinutes, (int)actual.TotalMinutes);
        }

        [Test]
        public void FirstDayOfWeekTest()
        {
            // Toegevoegd vanwege een fout in berekening FirstDayOfWeek op een zondag
            // Op zondag werd de komende maandag ipv afgelopen maandag als FirstDayOfWeek gebruikt
            for (var day = DateTime.Today; day < DateTime.Today.AddDays(7); day = day.AddDays(1))
            {
                Assert.IsTrue(day.FirstDayOfWeek() <= day, string.Format("{0} must be before {1}", day.FirstDayOfWeek(), day));
            }
        }


        /// <summary>
        ///A test for ThisWeek
        ///</summary>
        [Test]
        public void ThisWeekTest()
        {
            var expected = DateTime.Now -
                                DateTime.Today.AddDays(((DateTime.Today.DayOfWeek == DayOfWeek.Sunday
                                                             ? 7
                                                             : (int) DateTime.Today.DayOfWeek) - 1)*-1);
            var actual = PeriodBuilder.ThisWeek().Span;
            Console.WriteLine("Actual   {0} - {1}hrs - {2}min", actual.TotalDays, actual.TotalHours, actual.TotalMinutes);
            Console.WriteLine("Expected {0} - {1}hrs - {2}min", expected.TotalDays, expected.TotalHours, expected.TotalMinutes);
            Assert.AreEqual((int)expected.TotalMinutes, (int)actual.TotalMinutes); 
        }

        /// <summary>
        ///A test for Today
        ///</summary>
        [Test]
        public void TodayTest()
        {
            var expected = DateTime.Now - DateTime.Today;
            var actual = PeriodBuilder.Today().Span;
            Assert.AreEqual((int)expected.TotalMinutes, (int)actual.TotalMinutes);
        }

        /// <summary>
        ///A test for Yesterday
        ///</summary>
        [Test]
        public void YesterdayTest()
        {
            var expected = DateTime.Today - DateTime.Today.AddDays(-1);
            var actual = PeriodBuilder.Yesterday().Span;
            Assert.AreEqual((int)expected.TotalMinutes, (int)actual.TotalMinutes);
        }

        /// <summary>
        ///A test for Yesterday
        ///</summary>
        [Test]
        public void YesterdayRelativeTest()
        {
            var expected = DateTime.Now.AddDays(-1) - DateTime.Today.AddDays(-1);
            var actual = PeriodBuilder.YesterdayRelative().Span;
            Assert.AreEqual((int)expected.TotalMinutes, (int)actual.TotalMinutes);
        }
    }
}
