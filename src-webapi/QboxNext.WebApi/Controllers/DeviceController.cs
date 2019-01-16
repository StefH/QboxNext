﻿using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using Qboxes.Interfaces;
using QboxNext.Common.Validation;
using QBoxNext.Business.Interfaces.Public;
using QBoxNext.Business.Models;
using System.IO;
using System.Threading.Tasks;

namespace QboxNext.WebApi.Controllers
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;

        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
        private readonly IQboxMessagesLogger _qboxMessagesLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="qboxDataDataDumpContextFactory">The qbox data data dump context factory.</param>
        /// <param name="qboxMessagesLogger">The qbox messages logger.</param>
        /// <param name="logger">The logger.</param>
        public DeviceController([NotNull] IQboxDataDumpContextFactory qboxDataDataDumpContextFactory, [NotNull] IQboxMessagesLogger qboxMessagesLogger, [NotNull] ILogger<DeviceController> logger)
        {
            Guard.NotNull(qboxDataDataDumpContextFactory, nameof(qboxDataDataDumpContextFactory));
            Guard.NotNull(qboxMessagesLogger, nameof(qboxMessagesLogger));
            Guard.NotNull(logger, nameof(logger));

            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory;
            _qboxMessagesLogger = qboxMessagesLogger;
            _logger = logger;
        }

        // POST device/qbox
        // Example: /device/qbox/6618-1400-0200/15-46-002-442
        [HttpPost("/device/qbox/{productNumber}/{serialNumber}")]
        public async Task<ActionResult> PostAsync([NotNull] string productNumber, [NotNull] string serialNumber)
        {
            Guard.NotNullOrEmpty(productNumber, nameof(productNumber));
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));

            _logger.LogTrace("Enter");

            var context = await MapQboxContextAsync(productNumber, serialNumber);

            var qboxDataDumpContext = _qboxDataDumpContextFactory.Create(context);
            _logger.LogInformation(qboxDataDumpContext.Mini.SerialNumber);

            string result = new MiniDataHandler(qboxDataDumpContext, _qboxMessagesLogger).Handle();
            _logger.LogInformation("Parsing Done: {0}", result);
            _logger.LogTrace("Return");

            return Ok(result);
        }

        private async Task<QboxContext> MapQboxContextAsync(string productNumber, string serialNumber)
        {
            return new QboxContext
            {
                ProductNumber = productNumber,
                SerialNumber = serialNumber,
                LastSeenAtUrl = HttpContext.Request.Host.Value,
                ExternalIp = HttpContext.Connection.RemoteIpAddress.ToString(),
                Message = await ReadBodyAsync()
            };
        }

        private async Task<byte[]> ReadBodyAsync()
        {
            using (var memoryStream = new MemoryStream())
            {
                await HttpContext.Request.Body.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }
    }
}