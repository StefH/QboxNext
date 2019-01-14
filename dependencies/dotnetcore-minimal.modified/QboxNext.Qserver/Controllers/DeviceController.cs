using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Qboxes.Classes;
using Qboxes.Interfaces;
using Qboxes.Model.Classes;
using QboxNext.Core.Log;
using QboxNext.Qserver.Classes;

namespace QboxNext.Qserver.Controllers
{
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
        private readonly IQboxMessagesLogger _qboxMessagesLogger;
        private static readonly Logger Log = QboxNextLogFactory.GetLogger("DeviceController");

        public DeviceController(IQboxDataDumpContextFactory qboxDataDataDumpContextFactory)
        {
            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory;
            _qboxMessagesLogger = new QboxMessagesNullLogger();
        }

        // POST device/qbox
        // Example: /device/qbox/6618-1400-0200/15-46-002-442
        [HttpPost("/device/qbox/{pn}/{sn}")]
        public ActionResult Qbox(string pn, string sn)
        {
            Log.Trace("Enter");
            var qboxDataDumpContext = _qboxDataDumpContextFactory.CreateContext(ControllerContext, pn, sn);
            Log.Info(qboxDataDumpContext.Mini.SerialNumber);
            string result = new MiniDataHandler(qboxDataDumpContext, _qboxMessagesLogger).Handle();
            Log.Info("Parsing Done: {0}", result);
            Log.Trace("Return");
            return Content(result);
        }
    }
}
