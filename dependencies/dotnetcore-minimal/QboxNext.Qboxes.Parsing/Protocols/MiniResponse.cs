using Microsoft.Extensions.Logging;
using QboxNext.Logging;
using System.Linq;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// Parsing response string, ProtocolNr in BaseParseResult is not available in response string
    /// </summary>
    public class MiniResponse : MiniParser
    {
        private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger<MiniResponse>();

        protected override void DoParse()
        {
            Logger.LogTrace("Enter");

            var response = new ResponseParseResult();
            BaseParseResult = response;

            response.SequenceNr = Parser.ParseByte();
            response.ResponseTime = Parser.ParseTime();
            response.Offset = Parser.ParseByte();
            try
            {
                while (!Parser.EndOfStream)
                {
                    var setting = (DeviceSettingType)Parser.ParseByte();
                    // Below 0x40 is a devicesetting, above a (client) activity request or a client devicesetting. Parsing is different by 0x4? settings, writing is with clientnr reading without in A34
                    if ((byte)setting < 0x40)
                    {
                        foreach (var item in DeviceSettingsPayload.GetDeviceSettings(0, setting, Parser).ToList())
                            response.DeviceSettings.Add(item);
                    }
                    else
                    {
                        // refactor, see command.cs
                        response.DeviceSettings.Add(new DeviceSettingsPayload { DeviceSetting = setting, DeviceSettingValueStr = Parser.ReadToEnd() });
                    }
                }
            }
            catch
            {
                // suppress error
            }
            Logger.LogTrace("Exit");
        }
    }
}