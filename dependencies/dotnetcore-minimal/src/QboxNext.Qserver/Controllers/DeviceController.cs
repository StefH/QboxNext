using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QboxNext.Model.Classes;
using QboxNext.Model.Interfaces;
using QboxNext.Qboxes.Parsing;
using QboxNext.Qserver.Classes;

namespace QboxNext.Qserver.Controllers
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
        private readonly IParserFactory _parserFactory;
        private readonly ILogger<DeviceController> _logger;
        private readonly IQboxMessagesLogger _qboxMessagesLogger;

        public DeviceController(IQboxDataDumpContextFactory qboxDataDataDumpContextFactory, IParserFactory parserFactory, ILogger<DeviceController> logger)
        {
            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory ?? throw new ArgumentNullException(nameof(qboxDataDataDumpContextFactory));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _qboxMessagesLogger = new QboxMessagesNullLogger();
        }

        // POST device/qbox
        // Example: /device/qbox/6618-1400-0200/15-46-002-442
        [HttpPost("/device/qbox/{pn}/{sn}")]
        public ActionResult Qbox(string pn, string sn)
        {
            _logger.LogTrace("Enter");
            var qboxDataDumpContext = _qboxDataDumpContextFactory.CreateContext(ControllerContext, pn, sn);
            try
            {
                _logger.LogInformation(qboxDataDumpContext.Mini.SerialNumber);
                string result = new MiniDataHandler(qboxDataDumpContext, _qboxMessagesLogger, _parserFactory).Handle();
                _logger.LogInformation("Parsing Done: {0}", result);
                _logger.LogTrace("Return");
                return Content(result);
            }
            finally
            {
                foreach (var counterPoco in qboxDataDumpContext.Mini.Counters.Where(counterPoco => counterPoco.StorageProvider != null))
                {
                    counterPoco.StorageProvider.Dispose();
                }
            }
        }
    }
}
