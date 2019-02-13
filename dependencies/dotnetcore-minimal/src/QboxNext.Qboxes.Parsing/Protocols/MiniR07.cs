using Microsoft.Extensions.Logging;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Exceptions;
using System;
using System.Linq;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    public class MiniR07 : MiniParser
    {
        private readonly ILogger<MiniR07> _logger;

        public MiniR07(ILogger<MiniR07> logger, IProtocolReaderFactory protocolReaderFactory, SmartMeterCounterParser smartMeterCounterParser) : base(logger, protocolReaderFactory, smartMeterCounterParser)
        {
            _logger = logger;
        }

        protected override void DoParse()
        {
            _logger.LogTrace("Enter");

            BaseParseResult.ProtocolNr = Reader.ReadByte();
            BaseParseResult.SequenceNr = Reader.ReadByte();
            var model = new Mini07ParseModel
            {
                Status = new QboxMiniStatus(Reader.ReadByte()),
                MeasurementTime = Reader.ReadDateTime(),
                MeterType = (DeviceMeterType)Reader.ReadByte(),
                PayloadIndicator = new PayloaderIndicator(Reader.ReadByte())
            };

            if (!Enum.IsDefined(typeof(DeviceMeterType), model.MeterType))
                throw new BaseParserException(String.Format("Unexpected metertype: {0}", model.MeterType));
            if ((model.MeasurementTime - DateTime.Now).TotalHours > 3.0)
                throw new BaseParserException(String.Format("Unreliable time {0}", model.MeasurementTime));

            ParsePayload(model);

            var miniParseResult = BaseParseResult as MiniParseResult;
            if (miniParseResult != null) miniParseResult.Model = model;

            _logger.LogTrace("Exit");
        }

        private void ParsePayload(Mini07ParseModel model)
        {
            _logger.LogTrace("Enter");
            if (model.PayloadIndicator.ClientStatusPresent)
            {
                ParseClientStatuses(model);
            }
            _logger.LogDebug("DeviceSettingsPresent: {deviceSettingPresent}", model.PayloadIndicator.DeviceSettingPresent);
            if (model.PayloadIndicator.DeviceSettingPresent)
            {
                ParseDeviceSettings(model);
            }
            ParseCounters(model);
            if (model.PayloadIndicator.SmartMeterIsPresent)
            {
                var smartMeters = new[]
                                    {
                                        DeviceMeterType.Smart_Meter_EG, DeviceMeterType.Smart_Meter_E
                                    };
                if (smartMeters.Contains(model.MeterType))
                {
                    ParseSmartMeter(model);
                }
                else if (model.MeterType == DeviceMeterType.Soladin_600)
                {
                    ParseSoladin(model);
                }
                else
                {
                    throw new BaseParserException("Invalid metertype for message payload parsing");
                }
            }

            _logger.LogTrace("Exit");
        }

        protected virtual void ParseCounters(Mini07ParseModel model)
        {
            _logger.LogDebug("Nr of counters: {nrOfCounters}", model.PayloadIndicator.NrOfCounters);
            for (var i = 0; i < model.PayloadIndicator.NrOfCounters; i++)
            {
                try
                {
                    model.Payloads.Add(new CounterPayload
                    {
                        InternalNr = Reader.ReadByte(),
                        Value = Reader.ReadUInt32()
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Parsing counter payload (part {0} of {1})", i + 1, model.PayloadIndicator.NrOfCounters), ex);
                }
            }
        }

        private void ParseClientStatuses(MiniParseModel model)
        {
            try
            {
                // qplat-74: clientstatus R32
                byte NbrOfClientStatussen = Reader.ReadByte();
                _logger.LogDebug("Nr of client statuses: {nbrOfClientStatussen}", NbrOfClientStatussen);
                for (byte client = 0; client < NbrOfClientStatussen; client++)
                {
                    var clientId = BaseParseResult.ProtocolNr >= MiniR21.ProtocolVersion ? Reader.ReadByte() : client;
                    model.Payloads.Add(new ClientStatusPayload(model.MeasurementTime, clientId, Reader.ReadByte(), BaseParseResult.ProtocolNr));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Parsing client statussen", ex);
            }
        }

        private void ParseDeviceSettings(MiniParseModel model)
        {
            try
            {
                var setting = (DeviceSettingType)Reader.ReadByte();
                foreach (var item in DeviceSettingsPayload.GetDeviceSettings(BaseParseResult.ProtocolNr, setting, Reader).ToList())
                    model.Payloads.Add(item);
            }
            catch (Exception ex)
            {
                throw new Exception("Parsing device settings", ex);
            }
        }


        /// <summary>
        /// 09 78 07 0AADE9F4 09 80 00001100B6F30000C401B2018A13E8000000B100  50 09 00  3B443A000000007A
        /// </summary>
        /// <param name="model"></param>
        private void ParseSoladin(MiniParseModel model)
        {
            try
            {
                if (model == null) throw new ArgumentNullException("model");

                var source = Reader.ReadToEnd();
                if (string.IsNullOrEmpty(source))
                {
                    _logger.LogTrace("Nothing to parse (empty string), measurementTime: {measurementTime}", model.MeasurementTime);
                    return;
                }
                var value = source.Substring(40, 6);
                model.Payloads.Add(new CounterPayload
                {
                    InternalNr = 120,
                    Value = Read24bits(value)
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Parsing soladin", ex);
            }
        }

        private void ParseSmartMeter(MiniParseModel model)
        {
            try
            {
                if (model == null) throw new ArgumentNullException("model");

                var source = Reader.ReadToEnd();
                var strings = source.Split(':');
                if (string.IsNullOrEmpty(source))
                {
                    _logger.LogTrace("Nothing to parse (empty string), measurementTime: {measurementTime}", model.MeasurementTime);
                    return;
                }
                AddCounterPayload(model.Payloads, 181, strings.FirstOrDefault(s => s.Contains("1.8.1")), true);
                AddCounterPayload(model.Payloads, 182, strings.FirstOrDefault(s => s.Contains("1.8.2")), true);
                AddCounterPayload(model.Payloads, 281, strings.FirstOrDefault(s => s.Contains("2.8.1")), true);
                AddCounterPayload(model.Payloads, 282, strings.FirstOrDefault(s => s.Contains("2.8.2")), true);
                var raw =
                    strings.Where(
                        s =>
                        s.Contains("24.2.1)(m3)") || s.Contains("24.2.0)(m3)")).ToList();
                // Check on DSMR message for gas:
                if (raw.Count == 0)
                    raw =
                    strings.Where(
                        s =>
                        s.Contains("*m3")).ToList();
                AddCounterPayload(model.Payloads, 2421, raw, false);
            }
            catch (Exception ex)
            {
                throw new Exception("Parsing smart meter", ex);
            }
        }

        private ulong Read24bits(string value)
        {
            _logger.LogTrace("Enter - value: {value}", value);

            if (string.IsNullOrEmpty(value))
            {
                return ulong.MaxValue;
            }

            byte[] buffer = HexEncoding.HexStringToByteArray(value + "0000000000"); // todo: fix this quick fix... array length not sufficient for ulong
            //Array.Reverse(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
    }
}
