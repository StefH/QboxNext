using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Parsing response string, ProtocolNr in BaseParseResult is not available in response string
    /// </summary>
    public class MiniResponse : MiniParser
    {
        private readonly ILogger<MiniResponse> _logger;

        public MiniResponse(ILogger<MiniResponse> logger, IProtocolReaderFactory protocolReaderFactory, SmartMeterCounterParser smartMeterCounterParser) : base(logger, protocolReaderFactory, smartMeterCounterParser)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override void DoParse()
        {
            _logger.LogTrace("Enter");

            var response = new ResponseParseResult();
            BaseParseResult = response;

            response.SequenceNr = Reader.ReadByte();
            response.ResponseTime = Reader.ReadDateTime();
            response.Offset = Reader.ReadByte();
            try
            {
                while (!Reader.AtEndOfStream())
                {
                    var setting = (DeviceSettingType)Reader.ReadByte();
                    // Below 0x40 is a devicesetting, above a (client) activity request or a client devicesetting. Parsing is different by 0x4? settings, writing is with clientnr reading without in A34
                    if ((byte)setting < 0x40)
                    {
                        foreach (var item in DeviceSettingsPayload.GetDeviceSettings(0, setting, Reader).ToList())
                            response.DeviceSettings.Add(item);
                    }
                    else
                    {
                        // refactor, see command.cs
                        response.DeviceSettings.Add(new DeviceSettingsPayload { DeviceSetting = setting, DeviceSettingValueStr = Reader.ReadToEnd() });
                    }
                }
            }
            catch
            {
                // suppress error
            }
            _logger.LogTrace("Exit");
        }
    }
}