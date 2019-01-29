using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using QboxNext.Core;
using QboxNext.Core.Utils;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using QboxNext.Qserver.Core.Statistics;

namespace QboxNext.Model.Qboxes
{
    public class Counter
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger<Counter>();

        public int CounterId { get; set; }
        public IEnumerable<CounterDeviceMapping> CounterDeviceMappings { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public string StorageId { get; set; }
        public Qbox Qbox { get; set; }
        public CounterSource GroupId { get; set; }
        public bool Secondary { get; set; }

        /// <summary>
        /// In the original code this was a derived property, remove it by proper code to handle the situation
        /// when the Qbox is switched to a different meter (not supported now).
        /// </summary>
        public decimal Formula { get; set; }

        public IList<SeriesValue> GetSeries(Unit eenheid, DateTime from, DateTime to, SeriesResolution resolution, bool negate, bool smoothing)
        {
            Logger.LogTrace("Enter, counter = {0}, unit = {1}, from = {2}, to = {3}, resolution = {4}, negate = {5}, smoothing = {6}",
                CounterId, eenheid, from, to, resolution, negate, smoothing);

            Guard.IsBefore(from, to, "From cannot be later than to");
            var dispose = EnsureStorageProvider();
            try
            {
                if (resolution == SeriesResolution.FiveMinutes && CounterId == 2421)
                {
                    resolution = SeriesResolution.Hour;
                }
                var values = SeriesValueListBuilder.BuildSeries(from, to, resolution);
                if (CounterId == 2421)
                {
                    //todo: refactor hack, this can be properly handled when we have Energy and Power as units.
                    eenheid = Unit.M3;
                }

                var result = StorageProvider.GetRecords(from, to, eenheid, values, negate);
                if (smoothing)
                {
                    var data = values.OrderByDescending(o => o.Begin).ToList();
                    var currentIndex = 0;
                    foreach (var rec in data)
                    {
                        if (rec != null && rec.Value != null)
                        {
                            if (rec.Value == 0m)
                            {
                                var distance = 1;
                                while (currentIndex + distance < data.Count && data[currentIndex + distance].Value == 0m)
                                    distance++;
                                rec.Value = Math.Round(1m / FormulaForTime(rec.Begin) * 60m * 1000m) / (distance + 1m);
                            }
                            else if (currentIndex + 1 < data.Count && data[currentIndex + 1].Value == 0m)
                            {
                                var distance = 1;
                                while (currentIndex + distance < data.Count && data[currentIndex + distance].Value == 0m)
                                    distance++;
                                rec.Value = rec.Value / distance;
                            }
                        }
                        currentIndex++;
                    }
                }

                Logger.LogTrace("Return");
                return values;
            }
            finally
            {
                if (dispose)
                    DisposeStorageProvider();
            }
        }

        /// <summary>
        /// StorageId introduced for storing counter data based on Counterid, GroupId and secondary attributes
        /// GroupId and secondary attribute are new attributes for handling data from secondary meterdevice and (multiple)clients
        /// </summary>
        public void ComposeStorageid()
        {
            StorageId = String.Format("{0}_{1:00000000}{2}{3}",
                Qbox.SerialNumber,
                CounterId,
                GroupId == CounterSource.Host ? "" : "_" + GroupId.ToString(),
                Secondary ? "_secondary" : "");
        }

        private bool EnsureStorageProvider()
        {
            if (StorageProvider == null)
            {
                Debug.Assert(Qbox.SerialNumber != null, "Qbox.SerialNumber != null");

                StorageProvider = StorageProviderFactory.GetStorageProvider(false, Qbox.Storageprovider, Qbox.SerialNumber, Qbox.DataStore.Path, CounterId, Qbox.Precision, StorageId);
                return true;
            }
            return false;
        }

        private void DisposeStorageProvider()
        {
            StorageProvider.Dispose();
            StorageProvider = null;
        }

        public string QboxSerial { get; set; }

        public IEnumerable<CounterSensorMapping> CounterSensorMappings { get; set; }

        /// <summary>
        /// See Counter.ComposeStorageid
        /// </summary>
        /// <remarks>
        /// In the old situation the Storage ID would be computed by Counter (a database object) and copied (cached) in a CounterPoco.
        /// Now that we don't use a database, we need a way to correctly compose the Storage ID here as well.
        /// </remarks>
        public void ComposeStorageId()
        {
            StorageId = String.Format("{0}_{1:00000000}{2}{3}",
                QboxSerial,
                CounterId,
                GroupId == CounterSource.Host ? "" : "_" + GroupId.ToString(),
                Secondary ? "_secondary" : "");
        }

        //refactor: This is also implemented in the full counter object so maybe we should rethink this functionality
        /// <summary>
        /// Sets the value as received from the Qbox for the given measurement time.
        /// It uses the Storage Provider to actually store the data
        /// Every time the data is presented the LastValue property will be updated with the value.
        /// </summary>
        public void SetValue(DateTime measurementTime, ulong value, QboxStatus ioQboxStatus)
        {
            Guard.IsNotNull(StorageProvider, "storage provider is missing");

            var formula = FormulaForTime(measurementTime);
            var formulaEuro = RateForTime();
            var lastValue = GetLastRecord(measurementTime);

            Record runningTotal = StorageProvider.SetValue(measurementTime, value, formula, formulaEuro, lastValue);
            if (runningTotal != null)
            {
                runningTotal.Id = LastValueKey;
                ClientRepositories.MetaData?.Save(runningTotal);
            }

            if (lastValue != null && runningTotal != null && runningTotal.Raw > lastValue.Raw)
            {
                var counterKey = String.IsNullOrEmpty(StorageId) ? CounterId.ToString(CultureInfo.InvariantCulture) : StorageId;
                ioQboxStatus.LastValidDataReceivedPerCounter[counterKey] = DateTime.Now.ToUniversalTime();

                if (IsElectricityConsumptionCounter)
                    ioQboxStatus.LastElectricityConsumptionSeen = DateTime.Now.ToUniversalTime();
                else if (IsElectricityGenerationCounter)
                    ioQboxStatus.LastElectricityGenerationSeen = DateTime.Now.ToUniversalTime();
                else if (IsGasCounter)
                    ioQboxStatus.LastGasConsumptionSeen = DateTime.Now.ToUniversalTime();
                else
                    // We end up here for counters that can't be properly mapped to either LastElectricityConsumptionSeen, LastElectricityGenerationSeen or 
                    // LastGasConsumptionSeen. This doesn't mean that we couldn't handle the data. So to update the status to reflect that we saw SOME 
                    // valid data, we update LastElectricityConsumptionSeen.
                    ioQboxStatus.LastElectricityConsumptionSeen = DateTime.Now.ToUniversalTime();
            }
        }

        /// <summary>
        /// Get the last record from Redis or storage.
        /// </summary>
        public Record GetLastRecord(DateTime measurementTime)
        {
            // SAM: previously MetaData was set to read from Redis. For now we don't have a metadata database so we
            // work around it by always falling back on the previous value from the storage provider (which is an
            // expensive operation).
            Record lastValue = ClientRepositories.MetaData?.GetById<Record>(LastValueKey) ?? (measurementTime != DateTime.MinValue ? StorageProvider.FindPrevious(measurementTime) : null);
            return lastValue;
        }

        /// <summary>
        /// Entry point for later extension services using IoC / DI
        /// Should return the rate for calculating the Euro value of the 
        /// consumed or produced energy in the given timeframe
        /// </summary>
        /// <returns>The rate</returns>
        private decimal RateForTime()
        {
            return IsGasCounter ? 62m : 21m;
        }

        public bool IsGasCounter
        {
            get { return CounterId == GasCounterId; }
        }

        private bool IsElectricityConsumptionCounter
        {
            get { return CounterId == SmartConsumptionLowCounterId || CounterId == SmartConsumptionHighCounterId || IsFerrarisConsumptionCounter; }
        }

        private bool IsFerrarisConsumptionCounter
        {
            get { return CounterId == FerrarisLedConsumptionOrS0CounterId && !Secondary; }
        }

        /// <summary>
        /// Checks if the counter is a counter for separately measuring electricy generation.
        /// </summary>
        private bool IsElectricityGenerationCounter
        {
            get { return CounterId == SoladinGenerationCounterId || IsS0GenerationCounter; }
        }

        private bool IsS0GenerationCounter
        {
            get { return CounterId == FerrarisLedConsumptionOrS0CounterId && Secondary; }
        }

        /// <summary>
        /// Entry point for later extension services using IoC / DI
        /// Should return the formula for the calculation of the kWh according to that
        /// specific meter the counter is counting for
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        private decimal FormulaForTime(DateTime when)
        {
            var csm = CounterSensorMappings.Where(mapping => when > mapping.PeriodeBegin).OrderByDescending(o => o.PeriodeBegin).FirstOrDefault();
            if (csm == null)
                throw new Exception(String.Format("No active counter sensor mapping found for sn:counter {0} in period {1}", LastValueKey, when));

            return csm.Formule;
        }


        /// <summary>
        /// Get the key to use to store the last value in Redis.
        /// </summary>
        private string LastValueKey
        {
            get
            {
                if (string.IsNullOrEmpty(QboxSerial))
                    throw new InvalidDataException("Counter.QboxSerial has not been set, needed to compute key for last value of CounterPoco");
                return string.Format("{0}:{1}{2}{3}",
                    QboxSerial,
                    CounterId,
                    GroupId == CounterSource.Host ? "" : "_" + GroupId.ToString(),
                    Secondary ? "_secondary" : "");
            }
        }

        /// <summary>
        /// Remove cached last value.
        /// </summary>
        public void RemoveLastValue()
        {
            ClientRepositories.MetaData.Delete<Record>(LastValueKey);
        }


        public const int FerrarisLedConsumptionOrS0CounterId = 1;
        public const int SmartConsumptionLowCounterId = 181;
        public const int SmartConsumptionHighCounterId = 182;
        public const int SmartGenerationLowCounterId = 281;
        public const int SmartGenerationHighCounterId = 282;
        public const int SoladinGenerationCounterId = 120;
        public const int GasCounterId = 2421;
    }
}
