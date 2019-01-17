using System;
using System.Collections.Generic;
using QboxNext.Core.Utils;

namespace QboxNext.Qservice.Classes
{
    public interface ISeriesRetriever
    {
        /// <summary>
        /// Build the C# result that can be used to generate the Json result for GetSeries.
        /// </summary>
        IList<Serie> RetrieveForAccount(string inQboxSerial, DateTime inFromUtc, DateTime inToUtc, SeriesResolution inResolution);

        /// <summary>
        /// Build the C# result that can be used to generate the Json result for GetSeries.
        /// </summary>
        List<ValueSerie> RetrieveQboxSeries(RetrieveSeriesParameters parameters);
    }
}