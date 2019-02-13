using System.Linq;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Statistics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using QboxNext.Model.Classes;
using QboxNext.Storage;

namespace QboxNext.Qserver.Core
{
    /// <summary>
    ///This is a test class for SeriesValueListBuilderTest and is intended
    ///to contain all SeriesValueListBuilderTest Unit Tests
    ///</summary>
    [TestFixture()]
    public class SeriesValueListBuilderTest
    {
        /// <summary>
        ///A test for BuildSeries
        ///</summary>
        [Test]
        public void BuildSeriesShouldCreate5MinutesValueIntervalsTest()
        {
            // arrange
            var from = new DateTime(2012, 9, 11, 11, 2, 34);
            var to = new DateTime(2012, 9, 11, 11, 37, 34); 
            const SeriesResolution resolution = SeriesResolution.FiveMinutes;
            
            // act
            var actual = SeriesValueListBuilder.BuildSeries(@from, to, resolution);
            
            // assert
            Assert.AreEqual(8, actual.Count);
            Assert.AreEqual(0, actual[0].Begin.Minute);
            Assert.AreEqual(35, actual.Last().Begin.Minute);
            Assert.AreEqual(40, actual.Last().End.Minute);
        }

        /// <summary>
        ///A test for BuildSeries
        ///</summary>
        [Test]
        public void BuildSeriesShouldCreateDailyValueIntervalsTest()
        {
            // arrange
            var period = PeriodBuilder.ThisWeek();
            const SeriesResolution resolution = SeriesResolution.Day;

            // act
            var actual = SeriesValueListBuilder.BuildSeries(period.From, period.To, resolution);

            // assert
            Assert.AreEqual(Math.Round(period.Span.TotalDays + 0.5), actual.Count);
            Assert.AreEqual(0, actual[0].Begin.Minute);
            Assert.AreEqual(0, actual.Last().End.Minute);

            Data2Console(actual);
        }

        /// <summary>
        ///A test for BuildSeries
        ///</summary>
        [Test]
        public void BuildSeriesShouldCreateMonthlyValueOneYearIntervalsTest()
        {
            // arrange
            var period = PeriodBuilder.ThisYear();
            const SeriesResolution resolution = SeriesResolution.Month;

            // act
            var actual = SeriesValueListBuilder.BuildSeries(period.From, period.To, resolution);

            // assert
            Assert.AreEqual(DateTime.Now.Month, actual.Count);
            Assert.AreEqual(1, actual.First().Begin.Day);
            Assert.AreEqual(DateTime.Now.Day, actual.Last().End.Day);

            Data2Console(actual);
        }

        /// <summary>
        ///A test for BuildSeries
        ///</summary>
        [Test]
        public void BuildSeriesShouldCreateMonthlyValueFewMonthsIntervalsTest()
        {
            // arrange
            const SeriesResolution resolution = SeriesResolution.Month;

            // act
            var actual = SeriesValueListBuilder.BuildSeries(DateTime.Today.AddDays(-100), DateTime.Today, resolution);

            // assert
            Assert.AreEqual(DateTime.Today.AddDays(-100).Day, actual.First().Begin.Day);
            Assert.AreEqual(DateTime.Today.Day, actual.Last().End.Day);

            Data2Console(actual);
        }

        [Test]
        public void BuildSeriesShouldCreateWeekValueIntervalTest()
        {
            // arrange
            var period = new Period(DateTime.Now, DateTime.Now); // should create one week
            const SeriesResolution resolution = SeriesResolution.Week;

            // act
            var actual = SeriesValueListBuilder.BuildSeries(period.From, period.To, resolution);

            // assert
            Assert.AreEqual(1, actual.Count);

            Data2Console(actual);
        }

		[Test]
		public void BuildSeriesWithPartialMonthTest()
		{
			// arrange
			var start = new DateTime(2013, 1, 1);
			var end = new DateTime(2013, 1, 18, 10, 44, 00);

			// act
			var series = SeriesValueListBuilder.BuildSeries(start, end, SeriesResolution.Month);

			// assert
			Assert.AreEqual(1, series.Count);
			Assert.AreEqual(end, series[0].End);
		}

        private static void Data2Console(IEnumerable<SeriesValue> data)
        {
            foreach (var item in data)
                Console.WriteLine("{0} - {1}  {2}", item.Begin, item.End, item.Value);
        }
    }
}
