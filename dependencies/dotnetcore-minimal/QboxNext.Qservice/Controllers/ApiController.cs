using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QboxNext.Model.Interfaces;
using QboxNext.Qservice.Classes;

namespace QboxNext.Qservice.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly ISeriesRetriever _seriesRetriever;
        private readonly IMiniRetriever _miniRetriever;

        public ApiController(ILogger<ApiController> logger, IMiniRetriever miniRetriever)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _seriesRetriever = new SeriesRetriever();
            _miniRetriever = miniRetriever ?? throw new ArgumentNullException(nameof(miniRetriever));
        }

        [HttpGet("/api/getseries")]
        public ActionResult GetSeries(string sn, DateTime from, DateTime to, SeriesResolution? resolution)
        {
            // Sanitize from and to. Sometimes from is DateTime.MinValue for whatever reason, but earliest Qbox data is from 2012.
            var earliestDateWithNoDataForSure = new DateTime(2010, 1, 1);
            if (from < earliestDateWithNoDataForSure)
            {
                from = earliestDateWithNoDataForSure;
            }
            if (to < from)
            {
                to = from.AddDays(1);
            }

            var actualResolution = resolution ?? DeriveResolution(from, to);
            var fromUtc = DateTimeUtils.NlDateTimeToUtc(from);
            var toUtc = DateTimeUtils.NlDateTimeToUtc(to);
            var mini = _miniRetriever.Retrieve(sn);
            var series = _seriesRetriever.RetrieveForAccount(mini, fromUtc, toUtc, actualResolution);
            var response = new
            {
                result = true,
                data = series
            };
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Derives the series resolution from the given timeframe. The UI makes sure
        /// that when the customer requests data for the year graph, the @from and to are
        /// in different months. Otherwise the @from and to are in the same month.
        /// </summary>
        /// <param name="from">The start of the timeframe</param>
        /// <param name="to">The end of the timeframe</param>
        /// <returns>Series resolution</returns>
        private SeriesResolution DeriveResolution(DateTime from, DateTime to)
        {
            var span = to - from;
            _logger.LogDebug($"Series request span from and to: {span}");
            var firstDayToInclude = from;
            var lastDayToInclude = to.AddDays(-1);
            return span.TotalMinutes <= (int)SeriesResolution.Day ?
                span.TotalMinutes <= (int)SeriesResolution.Hour ? SeriesResolution.OneMinute : SeriesResolution.FiveMinutes :
                IsMostLikelyYearGraphRequestOrCustomPeriodTotalRequest(firstDayToInclude, lastDayToInclude) && !IsPossiblyWeekGraph(span) ? SeriesResolution.Month : SeriesResolution.Day;
        }

        private static bool IsMostLikelyYearGraphRequestOrCustomPeriodTotalRequest(DateTime firstDayToInclude, DateTime lastDayToInclude)
        {
            return ((firstDayToInclude.Month != lastDayToInclude.Month) || firstDayToInclude.Year != lastDayToInclude.Year);
        }

        private bool IsPossiblyWeekGraph(TimeSpan span)
        {
            return span.TotalMinutes == (int)SeriesResolution.Week;
        }
    }
}
