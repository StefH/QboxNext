using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QboxNext.Core;
using QboxNext.Core.Extensions;
using QboxNext.Core.Utils;
using QboxNext.Model.Classes;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Statistics;

namespace QboxNext.Qserver.Core.DataStore
{
    [TestFixture]
    [NonParallelizable]
    public class KWhStorageTest
    {
        private const string BaseDir = @"./Temp/KWhStorageTest";

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(BaseDir))
                Directory.Delete(BaseDir, true);

            Directory.CreateDirectory(BaseDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(BaseDir, true);
        }

        /// <summary>
        ///A test for SetValue
        ///</summary>
        [Test]
        public void SetValueShouldCreateAFileInTempTest()
        {
            // arrange
            const int counter = 1;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                var measureTime = DateTime.Now;

                // act
                target.SetValue(measureTime, 5, 5, 5);
            }
            // assert
            Assert.IsTrue(File.Exists(GetTestPath("Qbox_Serial/Serial_00000001.qbx")));
        }


        /// <summary>
        ///A test for SetValue
        ///</summary>
        [Test]
        public void SetValueShouldCreateAFileWithStorageidFilenameInTempTest()
        {
            // arrange
            const int counter = 1;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh, "MyStorageid-counterid"))
            {
                var measureTime = DateTime.Now;

                // act
                target.SetValue(measureTime, 5, 5, 5);
            }
            // assert
            Assert.IsTrue(File.Exists(GetTestPath("Qbox_Serial/MyStorageid-counterid.qbx")));
        }


        /// <summary>
        ///A test for SetValue
        ///</summary>
        [Test]
        public void SetValueShouldWriteTheRecordToFileAtOffsetTest()
        {
            // arrange
            string testPath = GetTestPath("Qbox_Serial/Serial_00000001.qbx");
            const int counter = 1;

            var measureTime = DateTime.Now;

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 25, 50, 15);
            }

            // assert

            Assert.IsTrue(File.Exists(testPath));
            const int offset = 32;
            using (var reader = new BinaryReader(File.OpenRead(testPath)))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                Assert.AreEqual(25, reader.ReadUInt64());
                Assert.AreEqual((ulong)(25m / 50m * 1000m), reader.ReadUInt64());
                Assert.AreEqual((ulong)(25m / 50m * 1000 * 15), reader.ReadUInt64());
                Assert.AreEqual((ulong)0, reader.ReadUInt16());

                Assert.AreEqual(ulong.MaxValue, reader.ReadUInt64());
                Assert.AreEqual(0, reader.ReadUInt64());
                Assert.AreEqual(0, reader.ReadUInt64());
                Assert.AreEqual((ulong)0, reader.ReadUInt16());
            }
        }


        [Test]
        [Category("Integration")]
        public void SetValueShouldReturnSecondTimeWithin5MsTest()
        {
            var watch = new Stopwatch();

            // arrange
            const int counter = 1;

            DateTime measureTime = DateTime.Now;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                // act first time with initialization
                target.SetValue(measureTime, 25, 50, 15);

                watch.Start();

                // act
                target.SetValue(measureTime.AddMinutes(1), 25, 50, 15);

                // assert
                watch.Stop();
                Console.WriteLine("elapsed: {0}", watch.ElapsedMilliseconds);
                Assert.IsTrue(watch.ElapsedMilliseconds < 5);
            }
        }


        [Test]
        public void SetValueShouldIncreaseTheFileSizeWithSevenDaysIfMeasureFallsBeyondTheEndOfTheFileTest()
        {
            // arrange
            const int counter = 1;

            var measureTime = DateTime.Now.AddDays(-10);

            var secondMeasureTime = DateTime.Now.AddDays(-1);

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 25, 50, 15);
                target.SetValue(secondMeasureTime, 25, 50, 15);
            }

            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {

                // assert
                Assert.That(target.StartOfFile.Equals(RoundDown(measureTime)));
                Assert.That(target.EndOfFile > secondMeasureTime);
                Assert.AreEqual((ulong)25, target.GetValue(secondMeasureTime).Raw);
                Assert.AreEqual(ulong.MaxValue, target.GetValue(secondMeasureTime.AddMinutes(1)).Raw);
            }
        }


        private static DateTime RoundDown(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }


        [Test]
        public void SetValueShouldIncreaseTheFileSizeWithOneYearIfMeasureIsOneMinuteBeyondTheEndOfTheFileTest()
        {
            // arrange
            const int counter = 1;

            var measureTime = new DateTime(DateTime.Today.Year - 1, 1, 1);
            var secondMeasureTime = new DateTime(DateTime.Today.Year, 1, 1);

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 25, 50, 15);
                target.SetValue(secondMeasureTime, 25, 50, 15);
            }

            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                // assert
                Assert.That(target.StartOfFile.Equals(RoundDown(measureTime)));
                Assert.AreEqual((ulong)25, target.GetValue(secondMeasureTime).Raw);
            }
        }


        /// <summary>
        ///A test for GetValue
        ///</summary>
        [Test]
        public void GetValueTest()
        {
            // arrange
            const int counter = 1;

            var measureTime = DateTime.Now;
            Record actual;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 50, 2, 3);
            }

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                actual = target.GetValue(measureTime);
            }

            // assert
            Assert.AreEqual(50, actual.Raw);
            Assert.AreEqual((ulong)(25), actual.KiloWattHour); // 50 / 2
            Assert.AreEqual((ulong)(75), actual.Money); // 50 / 2 * 3
        }


        [Test]
        public void GetValueShouldReturnNullIfTheTimeFallsOutsideTheCurrentFileRangeTest()
        {
            // arrange
            const int counter = 1;

            DateTime measureTime = DateTime.Now;
            Record actual;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 50, 5, 21);
            }

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                actual = target.GetValue(measureTime.AddMinutes(-1));
            }

            // assert
            Assert.IsNull(actual);
        }


        [Test]
        public void GetValueShouldReturnNullIfTheTimeFallsBeyondTheCurrentFileRangeTest()
        {
            // arrange
            const int counter = 1;

            var measureTime = DateTime.Now;
            Record actual;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 50, 25, 15);
            }

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                actual = target.GetValue(measureTime.AddYears(1));
            }

            // assert
            Assert.IsNull(actual);
        }

        // testen schrijven voor de gemiddelde uitvulling en de Value / Money formule

        [Test]
        public void GetValueShouldReturnTheAverageValueBetweenEmptySlotsTest()
        {
            // arrange
            const int counter = 1;

            var measureTime = DateTime.Now.AddMinutes(-15);
            Record actual = null;
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                target.SetValue(measureTime, 50, 25, 15);
                Console.WriteLine(target.GetValue(measureTime).Raw);
                target.SetValue(measureTime.AddMinutes(10), 100, 25, 15);
            }

            // act
            using (var target = new kWhStorage("Serial", BaseDir, counter, Precision.Wh))
            {
                for (var i = 0; i < 10; i++)
                {
                    actual = target.GetValue(measureTime.AddMinutes(i));
                    Console.WriteLine("Raw: {0} | Value: {1} | Quality: {2}", actual.Raw, actual.KiloWattHour, actual.Quality);
                }
            }
            // assert
            Assert.IsNotNull(actual, "actual != null");
            Assert.AreEqual(3.8m, actual.KiloWattHour);
            Assert.AreEqual((ushort)10000, actual.Quality);
        }


        [Test]
        public void QualityIndexShouldBeACertainValueBasedOnLogDistanceTest()
        {
            var intermediate = Math.Log10(5);
            var quality = Convert.ToUInt16(intermediate * 10000);
            Assert.AreEqual((ushort)6990, quality);
        }


        /// <summary>
        ///A test for Sum
        ///</summary>
        [Test]
        public void SumTest()
        {
            const string serialNumber = "Serial";
            const int counter = 1;
            decimal expected;
            decimal actual;
            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
            {
                target.SetValue(DateTime.Today.AddDays(-1), 50, 5, 2);
                target.SetValue(DateTime.Today.AddDays(-1).AddMinutes(120), 100, 5, 2);

                var period = PeriodBuilder.Yesterday();
                var begin = period.From;
                var end = period.To;

                expected = 10000; // ((100 / 5) * 1000) - ((50 / 5) * 1000) 
                actual = target.Sum(begin, end, Unit.kWh);
            }
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void GetValueShouldNotThrowExceptionWhenFileDoesNotExistTest()
        {
            // arrange
            const string serialNumber = "Serial";
            const int counter = 1;

            // act
            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
            {
                Record result = null;
                try
                {
                    result = target.GetValue(DateTime.Now);
                    // assert                    
                }
                catch
                {
                    Assert.Fail("Should not throw");
                }
                Assert.IsNull(result);
            }
        }


        [Test]
        public void GetRecordsShouldNotThrowWhenFileDoesNotExitTest()
        {
            // arrange
            const string serialNumber = "Serial";
            const int counter = 1;

            // act
            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
            {
                try
                {
                    var values = SeriesValueListBuilder.BuildSeries(DateTime.Today.AddDays(-7), DateTime.Today,
                                                                    SeriesResolution.Day);
                    var result = target.GetRecords(DateTime.Today.AddDays(-7), DateTime.Today, Unit.kWh, values, false);
                    // assert
                    Assert.IsFalse(result);
                    Assert.AreEqual(7, values.Count);
                    Assert.IsTrue(values.TrueForAll(v => v.Value == null));
                }
                catch
                {
                    Assert.Fail("Should not throw");
                }
            }
        }


        [Test]
        [Category("Integration")]
        public void GetRecordsShouldReturnWithinReasonableTimeWhenMeasurementsAreMissingfromTheFileTest()
        {
            // arrange
            const string serialNumber = "Serial";
            const int counter = 1;

            // Do this twice to miss the overhead of just in time compilation.
            for (int i = 0; i < 2; i++)
            {
                using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
                {
                    target.SetValue(DateTime.Now.AddDays(-8), 50, 1, 1);
                    target.SetValue(DateTime.Now.AddDays(-7), 100, 1, 1);

                    // act
                    var watch = new Stopwatch();
                    watch.Start();
                    var values = SeriesValueListBuilder.BuildSeries(DateTime.Today.AddDays(-30), DateTime.Today,
                                                                    SeriesResolution.Day);

                    var result = target.GetRecords(DateTime.Today.AddDays(-30), DateTime.Today, Unit.kWh, values, false);
                    watch.Stop();
                    var time = watch.ElapsedMilliseconds;

                    // assert
                    Assert.IsNotNull(result);

                    const int maxTime = 250;
                    if (time < maxTime)
                        break;

                    if (i > 0)
                        Assert.Less(time, maxTime); //Refactor the find algo because the file is too slow
                }
            }
        }


        [Test]
        public void GetSumTest()
        {
            // arrange
            const string serialNumber = "Serial";
            const int counter = 1;

            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
            {
                target.SetValue(DateTime.Now.AddDays(-8), 50, 1000, 1);
                target.SetValue(DateTime.Now.AddDays(-7), 100, 1000, 1);
                var begin = DateTime.Today.AddDays((DateTime.Today.Day - 1) * -1);
                var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                                       DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));

                // act
                var result = target.Sum(begin, end, Unit.kWh);

                // assert
                Assert.IsNotNull(result);
            }

        }


        [Test]
        public void WhenSetValueIsCalledWithAnIntervalItShouldLevelTheValuesWrittenTest()
        {
            // arrange
            const string serialNumber = "Interval";
            const int counter = 1;
            var start = DateTime.Now.AddMinutes(-2);

            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.mWh))
            {
                // act
                target.SetValue(start.AddMinutes(-10), 50, 300, 1);
                target.SetValue(start, 99, 300, 1);
                target.GetValue(start);
                // assert
                Assert.AreEqual(216.667m, target.GetValue(start.AddMinutes(-7)).KiloWattHour * 1000m);
                Assert.AreEqual(200.0m, target.GetValue(start.AddMinutes(-8)).KiloWattHour * 1000m);
                Assert.AreEqual(183.334m, target.GetValue(start.AddMinutes(-9)).KiloWattHour * 1000m);
                Assert.AreEqual(UInt64.MaxValue, target.GetValue(start.AddMinutes(-8)).Raw);
                Assert.AreEqual(UInt64.MaxValue, target.GetValue(start.AddMinutes(-1)).Raw);
                Assert.AreEqual(UInt64.MaxValue, target.GetValue(start.AddMinutes(-9)).Raw);
                Assert.AreEqual(99, target.GetValue(start).Raw);
                Assert.AreEqual(50, target.GetValue(start.AddMinutes(-10)).Raw);
            }
        }


        [Test]
        public void WhenSetValueIsCalledWith2000AsFormulaAndAPrecisionOf10000IsShouldWriteAHalfTest()
        {
            // arrange
            const string serialNumber = "Interval";
            const int counter = 1;
            var start = DateTime.Now.AddMinutes(-2);

            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.mWh))
            {
                // act
                target.SetValue(start.AddMinutes(-5), 5, 2000, 1);
                target.SetValue(start.AddMinutes(-4), 10, 2000, 1);
                target.SetValue(start.AddMinutes(-3), 15, 2000, 1);
                target.SetValue(start.AddMinutes(-2), 25, 2000, 1);
                target.SetValue(start.AddMinutes(-1), 26, 2000, 1);
                target.SetValue(start, 31, 2000, 1);

                target.GetValue(start);
                // assert
                Assert.AreEqual(25, target.GetValue(start.AddMinutes(-2)).Raw);
                Assert.AreEqual(26, target.GetValue(start.AddMinutes(-1)).Raw);
                var series = SeriesValueListBuilder.BuildSeries(start.AddMinutes(-5), start, SeriesResolution.OneMinute);

                target.GetRecords(start.AddMinutes(-5), start, Unit.kWh, series, false);

                Assert.AreEqual(300, series[2].Value);
                Assert.AreEqual(30, series[3].Value);
            }

        }


        [Test]
        public void WhenSumIsCalledItCalculatesTheSameAsTheIndividualResolutionTest()
        {
            // arrange
            const string serialNumber = "Interval";
            const int counter = 2421;
            var start = DateTime.Today.AddDays(-14);

            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.Wh))
            {
                var totalMinutes = (ulong)(DateTime.Now - start).TotalMinutes;
                for (ulong i = 0; i < totalMinutes; i++)
                {
                    target.SetValue(start.AddMinutes(i), (5 + (i * 2)), 1000, 10m);
                }

                // act
                target.GetValue(start);

                // assert
                var thisWeek = PeriodBuilder.LastWeek();
                var thisWeekSeries = SeriesValueListBuilder.BuildSeries(thisWeek.From, thisWeek.To, SeriesResolution.Hour);
                target.GetRecords(thisWeek.From, thisWeek.To, Unit.M3, thisWeekSeries, false);
                var thisWeekSeriesSum = thisWeekSeries.Sum(s => s.Value);

                var thisWeekSum = target.Sum(thisWeek.From, thisWeek.To, Unit.M3);
                Assert.AreEqual(thisWeekSum, thisWeekSeriesSum);

            }
        }


        [Test]
        public void WhenTheLastFiveMinutesAreRequestedFromTheFileItShouldCaluclateTheWeightForTheActualTimeGapTest()
        {
            // arrange
            const string serialNumber = "Interval";
            const int counter = 2421;
            var start = DateTime.Now.AddMinutes(-20);

            using (var target = new kWhStorage(serialNumber, BaseDir, counter, Precision.mWh))
            {
                // act
                var i = 0;
                while (start.AddMinutes(i) < DateTime.Now)
                {
                    target.SetValue(start.AddMinutes(i), (ulong)(5 * i), 1000, 21m);
                    i++;
                }

                target.GetValue(start);
                // assert
                var series = SeriesValueListBuilder.BuildSeries(DateTime.Today, DateTime.Now, SeriesResolution.FiveMinutes);

                target.GetRecords(DateTime.Today, DateTime.Now, Unit.kWh, series, false);
                Assert.AreEqual(300m, series[series.Count - 2].Value);
            }
        }


        [Test]
        public void SetValueShouldStoreTheRunningTotalsInTheFileTest()
        {
            // arrange
            var time = new DateTime(2012, 8, 17, 13, 20, 0);
            using (var storageProvider = new kWhStorage("12-13-001-075", BaseDir, 1, Precision.Wh))
            {
                // act
                for (int i = 0; i < 10; i++)
                {
                    storageProvider.SetValue(time.AddMinutes(i), (ulong)(50 * i), 1000m, 21m);
                }

            }
            using (var storageProvider = new kWhStorage("12-13-001-075", BaseDir, 1, Precision.Wh))
            {
                // assert
                string fileName = GetTestPath("Qbox_12-13-001-075/12-13-001-075_00000001.qbx");
                Assert.IsTrue(File.Exists(fileName));
                for (int i = 0; i < 10; i++)
                {
                    var actual = storageProvider.GetValue(time.AddMinutes(i));
                    Assert.AreEqual((ulong)(50 * i), actual.Raw);
                    Assert.AreEqual((ulong)(50 * i), actual.KiloWattHour * 1000m);
                }
            }
        }


        [Test]
        public void WhenLastDayOfStorageContainsPartialDataShouldGiveCorrectSum()
        {
            var time = new DateTime(2012, 8, 17, 13, 20, 0);
            using (var storageProvider = new kWhStorage("12-13-001-076", BaseDir, 1, Precision.Wh))
            {
                for (int i = 0; i < 10; i++)
                {
                    storageProvider.SetValue(time.AddMinutes(i), (ulong)(50 * i), 1000m, 21m);
                }

                Assert.IsTrue(storageProvider.Sum(storageProvider.StartOfFile, storageProvider.EndOfFile, Unit.kWh) > 0);
            }
        }


        [Test]
        public void WhenReadHeaderIsCalledStartTimeShouldBeReturned()
        {
            using (var storageProvider = new kWhStorage("00-00-000-002", BaseDir, 1, Precision.Wh))
            {
                storageProvider.SetValue(new DateTime(2013, 5, 7, 22, 58, 0), 3000, 300.0m, 0.21m);
                Assert.IsNotNull(storageProvider);
            }
        }


        [Test]
        public void WhenValueIsWrittenOneMinuteBeforeStartShouldThrowException()
        {
            DateTime startOfFile;
            DateTime endOfFile;

            Assert.Throws(typeof(InvalidOperationException), delegate
            {
                using (var storage = new kWhStorage("00-00-000-003", BaseDir, 1, Precision.Wh))
                {
                    // Write first value, this should set the start time of the storage.
                    storage.SetValue(new DateTime(2013, 5, 7, 22, 58, 0), 3000, 300.0m, 0.21m);
                    startOfFile = storage.StartOfFile;
                    endOfFile = storage.EndOfFile;
                    // Try to write a value one minute before the start.
                    var timestamp = startOfFile.AddMinutes(-1);
                    storage.SetValue(timestamp, 3000, 300.0m, 0.21m);
                }

                // If it doesn't throw an exception, we should be able to open the file again and the header should be intact.
                using (var storage = new kWhStorage("00-00-000-003", BaseDir, 1, Precision.Wh))
                {
                    storage.GetValue(new DateTime(2013, 5, 7, 22, 58, 0));

                    Assert.AreEqual(startOfFile, storage.StartOfFile);
                    Assert.AreEqual(endOfFile, storage.EndOfFile);
                }
            });
        }


        [Test]
        public void WhenValueIsWrittenInLastSlotShouldStoreValueAndNotGrowFile()
        {
            DateTime endOfFile;
            DateTime timestampAtEnd;

            using (var storage = new kWhStorage("00-00-000-003", BaseDir, 1, Precision.Wh))
            {
                // Write first value, this should set the start time of the storage.
                storage.SetValue(new DateTime(2013, 5, 7, 22, 58, 0), 3000, 300.0m, 0.21m);
                endOfFile = storage.EndOfFile;

                // Write a value in the last slot. Since the end of file is the end of the slot, we have to subtract one minute.
                timestampAtEnd = endOfFile.AddMinutes(-1);
                storage.SetValue(timestampAtEnd, 3000, 300.0m, 0.21m);
            }

            using (var storage = new kWhStorage("00-00-000-003", BaseDir, 1, Precision.Wh))
            {
                var record = storage.GetValue(timestampAtEnd);
                Assert.AreEqual(3000, record.Raw);
                Assert.AreEqual(endOfFile, storage.EndOfFile);
            }
        }


        [Test]
        public void WhenValueIsWrittenOneMinuteBeyondEndOfFileShouldGrowFile()
        {
            using (var storage = new kWhStorage("00-00-000-004", BaseDir, 1, Precision.Wh))
            {
                // Write first value, this should set the start time of the storage.
                storage.SetValue(new DateTime(2013, 5, 7, 22, 58, 0), 3000, 300.0m, 0.21m);
                var endOfFile = storage.EndOfFile;

                // Write a value just beyond the last slot.
                storage.SetValue(endOfFile, 3000, 300.0m, 0.21m);

                // This should have expanded the file.
                Assert.IsTrue(storage.EndOfFile > endOfFile);
            }
        }


        [Test]
        public void WhenValuesAreWrittenWithRunningTotalGetValueShouldReturnSameValues()
        {
            using (var storage = new kWhStorage("00-00-000-005", BaseDir, 1, Precision.Wh))
            {
                Record runningTotal = null;
                for (int i = 0; i < 10; ++i)
                    runningTotal = storage.SetValue(new DateTime(2013, 5, 7, 0, i, 0), (ulong)(3000 + i), 300.0m, 0.21m, runningTotal);
            }

            using (var storage = new kWhStorage("00-00-000-005", BaseDir, 1, Precision.Wh))
            {
                for (int i = 0; i < 10; ++i)
                {
                    var record = storage.GetValue(new DateTime(2013, 5, 7, 0, i, 0));
                    Assert.IsTrue(record.IsValidMeasurement);
                    Assert.AreEqual((ulong)(3000 + i), record.Raw);
                }
            }
        }


        [Test]
        public void WhenReinitializeIsCalledWithTimestampBeyondFileShouldNotThrowException()
        {
            using (var storage = new kWhStorage("00-00-000-004", BaseDir, 1, Precision.Wh))
            {
                // Write first value, this should set the start time of the storage.
                storage.SetValue(new DateTime(2013, 5, 7, 22, 58, 0), 3000, 300.0m, 0.21m);
                var endOfFile = storage.EndOfFile;

                storage.ReinitializeSlots(endOfFile.AddDays(1));
            }
        }


        [Test]
        public void WhenSlotEndsAfterEndOfFileButBeforeReferenceDateGetSeriesShouldReturnValue()
        {
            using (var storage = new kWhStorage("00-00-000-005", BaseDir, 1, Precision.Wh, "", false, 7))
            {
                var baseTimestamp = new DateTime(2013, 1, 1, 0, 0, 0);
                // Create a file that start and ends before now and that contains valid usage.
                storage.SetValue(baseTimestamp, 3000, 300.0m, 0.21m);
                storage.SetValue(baseTimestamp.AddHours(1), 6000, 300.0m, 0.21m);
                Assert.IsTrue(storage.EndOfFile < DateTime.Now);

                // If we now ask for a month of data, the slot will fall inside the file, but the end will not.
                // This should still result in a bit of consumption.
                var slots = SeriesValueListBuilder.BuildSeries(baseTimestamp, baseTimestamp.AddMonths(1), SeriesResolution.Month);
                Assert.AreEqual(1, slots.Count);
                Assert.IsNull(slots[0].Value);
                storage.GetRecords(baseTimestamp, baseTimestamp.AddMonths(1), Unit.kWh, slots, false);

                Assert.IsNotNull(slots[0].Value);
            }
        }


        [Test]
        [Category("Integration")]
        public void WhenReferenceDateIsBeforeEndOfFileGetSeriesShouldReturnValue()
        {
            // Test a new Qbox that has just started. The storage file starts at the first measurement, and ends after the ReferenceDate (now).
            using (var storage = new kWhStorage("00-00-000-005", BaseDir, 1, Precision.Wh, ""))
            {
                var baseTimestamp = DateTime.Now.Date;

                // Create a file that start and ends before now and that contains valid usage on the first day.
                storage.SetValue(baseTimestamp.AddHours(1), 3000, 300.0m, 0.21m);
                storage.SetValue(baseTimestamp.AddHours(2), 6000, 300.0m, 0.21m);

                // If we now ask for a week of day-data, the start of the first slot will be before the actual data,
                // and the end of the first slot will be after the actual data.
                // This should still result in a bit of consumption.
                var slots = SeriesValueListBuilder.BuildSeries(baseTimestamp, baseTimestamp.AddDays(7), SeriesResolution.Day);
                Assert.AreEqual(7, slots.Count);
                Assert.IsNull(slots[0].Value);

                storage.GetRecords(baseTimestamp, baseTimestamp.AddDays(7), Unit.kWh, slots, false);
                Assert.IsNotNull(slots[0].Value);
            }
        }


        [Test]
        public void WhenFirstTimestampOfSlotIsInvalidMeasurementPreviousValueShouldBeUsedTest()
        {
            using (var storage = new kWhStorage("00-00-000-006", BaseDir, 1, Precision.Wh, ""))
            {
                var firstZeroTimestamp = new DateTime(2013, 10, 4, 8, 22, 0);   // 2013-10-04 08:22
                storage.SetValue(firstZeroTimestamp, 0, 1000.0m, 0.21m);

                var lastZeroTimestamp = new DateTime(2014, 1, 4, 18, 14, 0);    // 2014-02-04 18:14
                var record = storage.SetValue(lastZeroTimestamp, 0, 1000.0m, 0.21m);

                var firstActualTimestamp = new DateTime(2014, 1, 5, 16, 46, 0); // 2014-02-05 16:46

                // Fake that running total is computed, but not written to file. This should mess up the interpolation which leads to
                // empty slots between lastZeroTimestamp and firstActualTimestamp.
                record.Time = firstActualTimestamp.AddMinutes(-1);

                storage.SetValue(firstActualTimestamp, 90913, 1000.0m, 0.21m, record);

                var startPeriod = new DateTime(2014, 1, 1);
                var endPeriod = new DateTime(2014, 2, 1);

                // Make sure we have data to fill each day in a month.
                var timestamp = firstActualTimestamp.Date.AddDays(1);
                ulong value = 100000;
                while (timestamp <= endPeriod)
                {
                    storage.SetValue(timestamp, value, 1000.0m, 0.21m);
                    value += 10000;
                    timestamp = timestamp.AddDays(1);
                }

                // Now check that we get a full month of data, even if some data is missing.
                var slots = SeriesValueListBuilder.BuildSeries(startPeriod, endPeriod, SeriesResolution.Day);
                Assert.IsTrue(storage.GetRecords(startPeriod, endPeriod, Unit.kWh, slots, false));

                foreach (var slot in slots)
                    Assert.IsNotNull(slot.Value);
            }
        }


        [Test]
        public void WhenFirstTimestampOfFirstSlotIsInvalidMeasurementNextValueShouldBeUsedTest()
        {
            using (var storage = new kWhStorage("00-00-000-006", BaseDir, 1, Precision.Wh, ""))
            {
                var firstZeroTimestamp = new DateTime(2013, 12, 31, 0, 0, 0);
                storage.SetValue(firstZeroTimestamp, 0, 1000.0m, 0.21m);

                var lastZeroTimestamp = new DateTime(2013, 12, 31, 23, 59, 0);
                var record = storage.SetValue(lastZeroTimestamp, 0, 1000.0m, 0.21m);

                var firstActualTimestamp = new DateTime(2014, 1, 1, 12, 0, 0);

                // Fake that running total is computed, but not written to file. This should mess up the interpolation which leads to
                // empty slots between lastZeroTimestamp and firstActualTimestamp.
                record.Time = firstActualTimestamp.AddMinutes(-1);

                storage.SetValue(firstActualTimestamp, 90913, 1000.0m, 0.21m, record);

                var startPeriod = new DateTime(2014, 1, 1);
                var endPeriod = new DateTime(2014, 2, 1);

                // Make sure we have data to fill each day in a month.
                var timestamp = firstActualTimestamp.Date.AddDays(1);
                ulong value = 100000;
                while (timestamp <= endPeriod)
                {
                    storage.SetValue(timestamp, value, 1000.0m, 0.21m);
                    value += 10000;
                    timestamp = timestamp.AddDays(1);
                }

                // Now check that we get a full month of data, even if some data is missing.
                var slots = SeriesValueListBuilder.BuildSeries(startPeriod, endPeriod, SeriesResolution.Day);
                Assert.IsTrue(storage.GetRecords(startPeriod, endPeriod, Unit.kWh, slots, false));

                foreach (var slot in slots)
                    Assert.IsNotNull(slot.Value);
            }
        }


        [Test]
        public void WhenFirstSlotIsInvalidRestOfSlotsShouldBeFilledTest()
        {
            using (var storage = new kWhStorage("00-00-000-006", BaseDir, 1, Precision.Wh, ""))
            {
                var firstZeroTimestamp = new DateTime(2013, 12, 31, 0, 0, 0);
                storage.SetValue(firstZeroTimestamp, 0, 1000.0m, 0.21m);

                var lastZeroTimestamp = new DateTime(2013, 12, 31, 23, 59, 0);
                var record = storage.SetValue(lastZeroTimestamp, 0, 1000.0m, 0.21m);

                var firstActualTimestamp = new DateTime(2014, 1, 2, 12, 0, 0);

                // Fake that running total is computed, but not written to file. This should mess up the interpolation which leads to
                // empty slots between lastZeroTimestamp and firstActualTimestamp.
                record.Time = firstActualTimestamp.AddMinutes(-1);

                storage.SetValue(firstActualTimestamp, 90913, 1000.0m, 0.21m, record);

                var startPeriod = new DateTime(2014, 1, 1);
                var endPeriod = new DateTime(2014, 2, 1);

                // Make sure we have data to fill each day in a month.
                var timestamp = firstActualTimestamp.Date.AddDays(1);
                ulong value = 100000;
                while (timestamp <= endPeriod)
                {
                    storage.SetValue(timestamp, value, 1000.0m, 0.21m);
                    value += 10000;
                    timestamp = timestamp.AddDays(1);
                }

                // Now check that we get a full month of data, even if some data is missing.
                var slots = SeriesValueListBuilder.BuildSeries(startPeriod, endPeriod, SeriesResolution.Day);
                Assert.IsTrue(storage.GetRecords(startPeriod, endPeriod, Unit.kWh, slots, false));

                Assert.IsNull(slots[0].Value);
                foreach (var slot in slots.Skip(1))
                    Assert.IsNotNull(slot.Value);
            }
        }


        [Test]
        public void FindPreviousWorksCorrectlyWithUntruncatedTimeTest()
        {
            using (var storage = new kWhStorage("00-00-000-006", BaseDir, 1, Precision.Wh, ""))
            {
                var baseTimestamp = new DateTime(2016, 2, 28, 18, 17, 16);

                storage.SetValue(baseTimestamp, 3000, 300.0m, 0.21m);
                storage.SetValue(baseTimestamp.AddHours(1), 6000, 300.0m, 0.21m);

                var record = storage.FindPrevious(baseTimestamp.AddMinutes(1));
                Assert.AreEqual(baseTimestamp.TruncateToMinute(), record.Time);
            }
        }

        private static string GetTestPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(BaseDir, relativePath));
        }
    }
}
