using CorrelationId;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using System.IO;
using System.Threading.Tasks;

namespace QboxNext.Server.WebApi.Controllers
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
        private readonly IQboxNextDataHandlerFactory _qboxNextDataHandlerFactory;
        private readonly ICorrelationContextAccessor _correlationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceController"/> class.
        /// </summary>
        /// <param name="qboxDataDataDumpContextFactory">The qbox data data dump context factory.</param>
        /// <param name="qboxNextDataHandlerFactory">The qbox next data handler factory.</param>
        /// <param name="correlationContext">The correlation context.</param>
        /// <param name="logger">The logger.</param>
        public DeviceController(
            [NotNull] IQboxDataDumpContextFactory qboxDataDataDumpContextFactory,
            [NotNull] IQboxNextDataHandlerFactory qboxNextDataHandlerFactory,
            [NotNull] ICorrelationContextAccessor correlationContext,
            [NotNull] ILogger<DeviceController> logger)
        {
            Guard.IsNotNull(qboxDataDataDumpContextFactory, nameof(qboxDataDataDumpContextFactory));
            Guard.IsNotNull(qboxNextDataHandlerFactory, nameof(qboxNextDataHandlerFactory));
            Guard.IsNotNull(correlationContext, nameof(correlationContext));
            Guard.IsNotNull(logger, nameof(logger));

            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory;
            _qboxNextDataHandlerFactory = qboxNextDataHandlerFactory;
            _correlationContext = correlationContext;
            _logger = logger;
        }

        // POST device/qbox
        // Example: /device/qbox/6618-1400-0200/15-46-002-442
        [HttpPost(@"/device/qbox/{productNumber:regex(^\d{{4}}-\d{{4}}-\d{{4}}$)}/{serialNumber:regex(^\d{{2}}-\d{{2}}-\d{{3}}-\d{{3}}$)}")]
        public async Task<ActionResult> PostAsync([NotNull] string productNumber, [NotNull] string serialNumber)
        {
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            _logger.LogInformation($"PostAsync /device/qbox/{productNumber}/{serialNumber}");

            // Create QboxContext
            var context = await MapQboxContextAsync(productNumber, serialNumber);

            // Create QboxDataDumpContext
            var qboxDataDumpContext = _qboxDataDumpContextFactory.Create(context);

            // Create handler and handle the message
            var handler = _qboxNextDataHandlerFactory.Create(_correlationContext.CorrelationContext.CorrelationId, qboxDataDumpContext);
            string result = await handler.HandleAsync();

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
