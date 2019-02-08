using System;
using System.Collections.Generic;
using System.Linq;
using QboxNext.Core.Utils;
using QboxNext.Core.Extensions;
using QboxNext.Model.Qboxes;
using QboxNext.Storage;

namespace QboxNext.Qservice.Classes
{
    /// <summary>
    /// Class to retrieve live data from QBX files.
    /// </summary>
    public class LiveDataRetriever
    {
        private readonly ISeriesRetriever _seriesRetriever;


        public LiveDataRetriever(ISeriesRetriever seriesRetriever)
        {
            _seriesRetriever = seriesRetriever;
        }


        /// <summary>
        /// Retrieve live energy data for an account.
        /// </summary>
        /// <remarks>
        /// Does not check read permissions.
        /// </remarks>
        /// <returns>
        /// Live data for each active device of the account.
        /// </returns>
        public IList<LiveDataForDevice> Retrieve(Mini mini, DateTime nowUtc)
        {
            var toUtc = nowUtc.Truncate(TimeSpan.FromMinutes(1));
            var fromUtc = toUtc.AddMinutes(-5);

            var liveData = new List<LiveDataForDevice>();
            IEnumerable<ValueSerie> seriesList = _seriesRetriever.RetrieveSerieValuesForAccount(mini, fromUtc, toUtc, SeriesResolution.OneMinute);
            foreach (var series in seriesList)
            {
                decimal? power = Series2Power(series.Data);
                liveData.Add(new LiveDataForDevice
                {
                    EnergyType = series.EnergyType,
                    Name = series.Name,
                    Power = power
                });
            }

            return liveData;
        }


        private static decimal? Series2Power(IEnumerable<SeriesValue> values)
        {
            var items = values.Where(s => s.Value != null).ToList();
            return !items.Any() ? null : items.Last().Value;
        }
    }
}
