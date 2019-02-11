using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using QboxNext.Core.Utils;

namespace QboxNext.Qservice.Utils
{
    public class ResolutionCalculatorTest
    {
        private static readonly ResolutionCalculator _calculator = new ResolutionCalculator();

        // One minute resolution for short spans:
        [TestCase("2019-01-01T00:00:00", "2019-01-01T00:00:01", SeriesResolution.OneMinute)]
        [TestCase("2019-01-01T00:00:00", "2019-01-01T00:05:00", SeriesResolution.OneMinute)]
        [TestCase("2019-01-01T00:00:00", "2019-01-01T01:00:00", SeriesResolution.OneMinute)]
        // 5 minute resolution for day graph:
        [TestCase("2019-01-01", "2019-01-02", SeriesResolution.FiveMinutes)]
        // Switch to/from winter time, day has 25/23 hours. Note that DateTime does not handle this, day has 24 hours:
        [TestCase("2018-10-28", "2018-10-29", SeriesResolution.FiveMinutes)]
        [TestCase("2018-03-25", "2018-03-26", SeriesResolution.FiveMinutes)]
        // day resolution for week graph:
        [TestCase("2018-01-01", "2018-01-08", SeriesResolution.Day)]
        // day resolution for month graph:
        [TestCase("2018-01-01", "2018-02-01", SeriesResolution.Day)]
        // month resolution for year graph:
        [TestCase("2018-01-01", "2019-01-01", SeriesResolution.Month)]
        [TestCase("2012-01-01", "2019-01-01", SeriesResolution.Month)]
        public void TestCalculate(DateTime from, DateTime to, SeriesResolution expectedResolution)
        {
            Assert.That(_calculator.Calculate(from, to), Is.EqualTo(expectedResolution));
        }
    }
}
