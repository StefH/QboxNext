using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLog.Fluent;
using QboxNext.Core;
using QboxNext.Core.Utils;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Factories;
using QboxNext.Qserver.Core.Interfaces;
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

                Log.Trace("Return");
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

        private decimal FormulaForTime(DateTime time)
        {
            return Formula;
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
    }
}
