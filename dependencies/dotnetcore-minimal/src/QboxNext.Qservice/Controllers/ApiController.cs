using System;
using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QboxNext.Model.Interfaces;
using QboxNext.Model.Qboxes;
using QboxNext.Qservice.Classes;
using QboxNext.Qservice.Utils;

namespace QboxNext.Qservice.Controllers
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly ISeriesRetriever _seriesRetriever;
        private readonly IMiniRetriever _miniRetriever;
        private readonly ResolutionCalculator _resolutionCalculator = new ResolutionCalculator();

        public ApiController(ILogger<ApiController> logger, IMiniRetriever miniRetriever, ISeriesRetriever seriesRetriever)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _miniRetriever = miniRetriever ?? throw new ArgumentNullException(nameof(miniRetriever));
            _seriesRetriever = seriesRetriever ?? throw new ArgumentNullException(nameof(seriesRetriever));
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
            var series = RetrieveForAccount(mini, fromUtc, toUtc, actualResolution);
            var response = new
            {
                result = true,
                data = series
            };
            return new OkObjectResult(response);
        }

        /// <summary>
        /// Derives the series resolution from the given time frame. The UI makes sure
        /// that when the customer requests data for the year graph, the @from and to are
        /// in different months. Otherwise the @from and to are in the same month.
        /// </summary>
        /// <param name="from">The start of the time frame</param>
        /// <param name="to">The end of the time frame</param>
        /// <returns>Series resolution</returns>
        private SeriesResolution DeriveResolution(DateTime from, DateTime to)
        {
            var span = to - from;
            _logger.LogDebug($"Series request span from and to: {span}");
            return _resolutionCalculator.Calculate(from, to);
        }

        /// <summary>
        /// Build the C# result that can be used to generate the Json result for GetSeries.
        /// </summary>
        private IList<Serie> RetrieveForAccount(Mini mini, DateTime inFromUtc, DateTime inToUtc, SeriesResolution inResolution)
        {
            var valueSeries = _seriesRetriever.RetrieveSerieValuesForAccount(mini, inFromUtc, inToUtc, inResolution);
            return Mapper.Map<IEnumerable<ValueSerie>, IList<Serie>>(valueSeries);
        }


        /// <summary>
        /// Get all data related to current power usage or generation.
        /// </summary>
        [HttpGet("/api/getlivedata")]
        public ActionResult GetLiveData(string sn)
        {
            var liveDataRetriever = new LiveDataRetriever(_seriesRetriever);
            var mini = _miniRetriever.Retrieve(sn);
            var data = liveDataRetriever.Retrieve(mini, DateTime.UtcNow);

            return new OkObjectResult(new { result = true, data });
        }
    }
}
