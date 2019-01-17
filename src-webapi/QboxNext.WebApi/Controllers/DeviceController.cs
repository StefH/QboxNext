using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using System.IO;
using System.Threading.Tasks;

namespace QboxNext.WebApi.Controllers
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;

        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
        private readonly IQboxNextDataHandlerFactory _qboxNextDataHandlerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="qboxDataDataDumpContextFactory">The qbox data data dump context factory.</param>
        /// <param name="qboxNextDataHandlerFactory">The qbox messages logger.</param>
        /// <param name="logger">The logger.</param>
        public DeviceController([NotNull] IQboxDataDumpContextFactory qboxDataDataDumpContextFactory, [NotNull] IQboxNextDataHandlerFactory qboxNextDataHandlerFactory, [NotNull] ILogger<DeviceController> logger)
        {
            Guard.IsNotNull(qboxDataDataDumpContextFactory, nameof(qboxDataDataDumpContextFactory));
            Guard.IsNotNull(qboxNextDataHandlerFactory, nameof(qboxNextDataHandlerFactory));
            Guard.IsNotNull(logger, nameof(logger));

            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory;
            _qboxNextDataHandlerFactory = qboxNextDataHandlerFactory;
            _logger = logger;
        }

        // POST device/qbox
        // Example: /device/qbox/6618-1400-0200/15-46-002-442
        [HttpPost("/device/qbox/{productNumber}/{serialNumber}")]
        public async Task<ActionResult> PostAsync([NotNull] string productNumber, [NotNull] string serialNumber)
        {
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _logger.LogTrace("Enter");

            // Create QboxContext
            var context = await MapQboxContextAsync(productNumber, serialNumber);

            // Create QboxDatDumpContext
            var qboxDataDumpContext = _qboxDataDumpContextFactory.Create(context);
            _logger.LogInformation(qboxDataDumpContext.Mini.SerialNumber);

            // Create handler and handle the message
            var handler = _qboxNextDataHandlerFactory.Create(qboxDataDumpContext);
            string result = await handler.HandleAsync();

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
