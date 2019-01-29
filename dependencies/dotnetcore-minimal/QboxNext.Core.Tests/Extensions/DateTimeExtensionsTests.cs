using System;
using System.Xml;
using FluentAssertions;
using NUnit.Framework;

namespace QboxNext.Core.Extensions
{
    public class DateTimeExtensionsTests
    {
        [TestCase("2018-05-17T21:12:34", "2018-05-01T00:00:00")]
        [TestCase("2018-05-01T00:00:00", "2018-05-01T00:00:00")]
        [TestCase("2020-02-29T00:00:00", "2020-02-01T00:00:00")]
        public void Given_any_date_should_return_correct_first_day_of_month_as_date(string given, string expected)
        {
            DateTime givenDate = XmlConvert.ToDateTime(given, XmlDateTimeSerializationMode.Unspecified);
            DateTime expectedDate = XmlConvert.ToDateTime(expected, XmlDateTimeSerializationMode.Unspecified);

            // Act
            DateTime actual = givenDate.FirstDayOfMonth();

            // Assert
            actual.Should().Be(expectedDate);
        }

        [TestCase("2018-05-17T21:12:34", "2018-05-31T00:00:00")]
        [TestCase("2018-05-01T00:00:00", "2018-05-31T00:00:00")]
        [TestCase("2020-02-29T00:00:00", "2020-02-29T00:00:00")]
        public void Given_any_date_should_return_correct_last_day_of_month_as_date(string given, string expected)
        {
            DateTime givenDate = XmlConvert.ToDateTime(given, XmlDateTimeSerializationMode.Unspecified);
            DateTime expectedDate = XmlConvert.ToDateTime(expected, XmlDateTimeSerializationMode.Unspecified);

            // Act
            DateTime actual = givenDate.LastDayOfMonth();

            // Assert
            actual.Should().Be(expectedDate);
        }

        [TestCase("2018-05-17T21:12:34", "2018-05-14T00:00:00")]
        [TestCase("2018-05-01T00:00:00", "2018-04-30T00:00:00")]
        [TestCase("2020-02-29T00:00:00", "2020-02-24T00:00:00")]
        [TestCase("2020-01-27T00:00:00", "2020-01-27T00:00:00")]
        public void Given_any_date_should_return_correct_first_day_of_week_as_date(string given, string expected)
        {
            DateTime givenDate = XmlConvert.ToDateTime(given, XmlDateTimeSerializationMode.Unspecified);
            DateTime expectedDate = XmlConvert.ToDateTime(expected, XmlDateTimeSerializationMode.Unspecified);

            // Act
            DateTime actual = givenDate.FirstDayOfWeek();

            // Assert
            actual.Should().Be(expectedDate);
        }

        [TestCase("2018-05-17T21:12:34", "2018-05-20T00:00:00")]
        [TestCase("2018-05-01T00:00:00", "2018-05-06T00:00:00")]
        [TestCase("2020-02-29T00:00:00", "2020-03-01T00:00:00")]
        [TestCase("2020-01-27T00:00:00", "2020-02-02T00:00:00")]
        public void Given_any_date_should_return_correct_last_day_of_week_as_date(string given, string expected)
        {
            DateTime givenDate = XmlConvert.ToDateTime(given, XmlDateTimeSerializationMode.Unspecified);
            DateTime expectedDate = XmlConvert.ToDateTime(expected, XmlDateTimeSerializationMode.Unspecified);

            // Act
            DateTime actual = givenDate.LastDayOfWeek();

            // Assert
            actual.Should().Be(expectedDate);
        }
    }
}