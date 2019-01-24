using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using QboxNext.Core.Utils;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qserver.Core.Model
{
    /// <summary>
    /// Counter data holder
    /// </summary>
    public class CounterPoco
    {
        public string QboxSerial { get; set; }
        public IStorageProvider StorageProvider { get; set; }
        public int CounterId { get; set; }

        public CounterSource GroupId { get; set; }
        public bool Secondary { get; set; }
        public string StorageId { get; set; }

        public IEnumerable<CounterSensorMappingPoco> CounterSensorMappings { get; set; }

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

        #region Private

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
                    throw new InvalidDataException("CounterPoco.QboxSerial has not been set, needed to compute key for last value of CounterPoco");
                return string.Format("{0}:{1}{2}{3}",
                    QboxSerial,
                    CounterId,
                    GroupId == CounterSource.Host ? "" : "_" + GroupId.ToString(),
                    Secondary ? "_secondary" : "");
            }
        }


        #endregion

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