//using System;
//using System.IO;
//using Microsoft.AspNetCore.Mvc;
//using NLog;
//using Qboxes.Classes;
//using Qboxes.Interfaces;
//using Qboxes.Model.Classes;
//using QboxNext.Core.Encryption;
//using QboxNext.Core.Log;
//using QboxNext.Qboxes.Parsing.Factories;
//using QboxNext.Qboxes.Parsing.Protocols;
//using QboxNext.Qserver.Classes;

//namespace QboxNext.Qserver.Controllers
//{
//    [ApiController]
//    public class DeviceController2 : ControllerBase
//    {
//        private readonly IQboxDataDumpContextFactory _qboxDataDumpContextFactory;
//        private readonly IQboxMessagesLogger _qboxMessagesLogger;
//        private static readonly Logger Log = QboxNextLogFactory.GetLogger("DeviceController");

//        public DeviceController2(IQboxDataDumpContextFactory qboxDataDataDumpContextFactory)
//        {
//            _qboxDataDumpContextFactory = qboxDataDataDumpContextFactory;
//            _qboxMessagesLogger = new QboxMessagesNullLogger();
//        }

//        // POST device/qbox
//        // Example: /device/qbox/6618-1400-0200/15-46-002-442
//        [HttpPost("/device/qbox/{pn}/{sn}")]
//        public ActionResult Qbox(string pn, string sn)
//        {
//            string lastSeenAtUrl = Request.Host.Value;
//            string externalIp = HttpContext.Connection.RemoteIpAddress.ToString();

//            using (var memoryStream = new MemoryStream())
//            {
//                HttpContext.Request.Body.CopyTo(memoryStream);

//                long length = memoryStream.Length;

//                byte[] bytes = memoryStream.ToArray();

//                string message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(bytes);

//                Log.Trace($"lastSeenAtUrl: {lastSeenAtUrl}");
//                Log.Trace($"externalIp: {externalIp}");
//                Log.Trace($"length: {length}");
//                Log.Trace($"message: {message}");


//                // start parsing

//                var parser = ParserFactory.GetParserFromMessage(message);
//                var _result = parser.Parse(message);

//                // end of parsing

//                if ((_result as MiniParseResult) != null)
//                {
//                    var parseResult = (_result as MiniParseResult);

//                    // handle the result
//                    byte FirmwareVersion = parseResult.ProtocolNr;
//                    MiniState ministate = parseResult.Model.Status.Status;

//                    bool operational = false;
//                    switch (ministate)
//                    {
//                        case MiniState.HardReset:
//                            // _context.Mini.QboxStatus.LastHardReset = DateTime.UtcNow;
//                            break;
//                        case MiniState.InvalidImage:
//                            // _context.Mini.QboxStatus.LastImageInvalid = DateTime.UtcNow;
//                            break;
//                        case MiniState.Operational:
//                            operational = true;
//                            break;
//                        case MiniState.ValidImage:
//                            // _context.Mini.QboxStatus.LastImageValid = DateTime.UtcNow;
//                            break;
//                        case MiniState.UnexpectedReset:
//                            // _context.Mini.QboxStatus.LastPowerLoss = DateTime.UtcNow;
//                            break;
//                    }

//                    //if (!operational)
//                    //    _context.Mini.QboxStatus.LastNotOperational = DateTime.UtcNow;

//                    //if (parseResult.Model.Status.TimeIsReliable)
//                    //    _context.Mini.QboxStatus.LastTimeIsReliable = DateTime.UtcNow;
//                    //else
//                    //    _context.Mini.QboxStatus.LastTimeUnreliable = DateTime.UtcNow;

//                    //if (parseResult.Model.Status.ValidResponse)
//                    //    _context.Mini.QboxStatus.LastValidResponse = DateTime.UtcNow;
//                    //else
//                    //    _context.Mini.QboxStatus.LastInvalidResponse = DateTime.UtcNow;

//                    foreach (var payload in parseResult.Model.Payloads)
//                    {
//                        // payload.Visit();
//                    }
//                }
//            }

//            Log.Trace("Enter");
//            var qboxDataDumpContext = _qboxDataDumpContextFactory.CreateContext(ControllerContext, pn, sn);
//            Log.Info(qboxDataDumpContext.Mini.SerialNumber);
//            string result = new MiniDataHandler(qboxDataDumpContext, _qboxMessagesLogger).Handle();
//            Log.Info("Parsing Done: {0}", result);
//            Log.Trace("Return");
//            return Content(result);
//        }
//    }
//}
