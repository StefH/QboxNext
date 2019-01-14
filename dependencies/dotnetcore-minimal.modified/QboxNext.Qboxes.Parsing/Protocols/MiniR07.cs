using System;
using System.Linq;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Exceptions;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class MiniR07 : MiniParser
	{
		private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

		protected override void DoParse()
        {
            Log.Trace("Enter");

            BaseParseResult.ProtocolNr = Parser.ParseByte();
            BaseParseResult.SequenceNr = Parser.ParseByte();
            var model = new Mini07ParseModel
                {
                    Status = new QboxMiniStatus(Parser.ParseByte()),
                    MeasurementTime = Parser.ParseTime(),
                    MeterType = (DeviceMeterType)Parser.ParseByte(),
                    PayloadIndicator = new PayloaderIndicator(Parser.ParseByte())
                };

			if (!Enum.IsDefined(typeof(DeviceMeterType), model.MeterType))
				throw new BaseParserException(String.Format("Unexpected metertype: {0}", model.MeterType));
			if ((model.MeasurementTime - DateTime.Now).TotalHours > 3.0)
				throw new BaseParserException(String.Format("Unreliable time {0}", model.MeasurementTime));

            ParsePayload(model);

            var miniParseResult = BaseParseResult as MiniParseResult;
            if (miniParseResult != null) miniParseResult.Model = model;

            Log.Trace("Exit");
        }

        private void ParsePayload(Mini07ParseModel model)
        {
            Log.Trace("Enter");
            if (model.PayloadIndicator.ClientStatusPresent)
            {
                ParseClientStatuses(model);
            }
            Log.DebugFormat("DeviceSettingsPresent: {deviceSettingPresent}", model.PayloadIndicator.DeviceSettingPresent);
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
            
            Log.Trace("Exit");
        }

        protected virtual void ParseCounters(Mini07ParseModel model)
        {
            Log.DebugFormat("Nr of counters: {nrOfCounters}", model.PayloadIndicator.NrOfCounters);
            for (var i = 0; i < model.PayloadIndicator.NrOfCounters; i++)
            {
                try
                {
					model.Payloads.Add(new CounterPayload
					{
						InternalNr = Parser.ParseByte(),
						Value = Parser.ParseUInt32()
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
                var NbrOfClientStatussen = Parser.ParseByte();
                Log.DebugFormat("Nr of client statuses: {nbrOfClientStatussen}", NbrOfClientStatussen);
                for (byte client = 0; client < NbrOfClientStatussen; client++)
                {
					var clientId = BaseParseResult.ProtocolNr >= MiniR21.ProtocolVersion ? Parser.ParseByte() : client;
					model.Payloads.Add(new ClientStatusPayload(model.MeasurementTime, clientId, Parser.ParseByte(), BaseParseResult.ProtocolNr));
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
                var setting = (DeviceSettingType)Parser.ParseByte();
                foreach(var item in DeviceSettingsPayload.GetDeviceSettings(BaseParseResult.ProtocolNr, setting, Parser).ToList())
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

                var source = Parser.ReadToEnd();
                if (string.IsNullOrEmpty(source))
                {
                    Log.TraceFormat("Nothing to parse (empty string), measurementTime: {measurementTime}", model.MeasurementTime);
                    return;
                }
                var value = source.Substring(40, 6);
                model.Payloads.Add(new CounterPayload
                                       {
                                           InternalNr = 120,
                                           Value = Parser.Read24bits(value)
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

                var source = Parser.ReadToEnd();
                var strings = source.Split(':');
                if (string.IsNullOrEmpty(source))
                {
                    Log.TraceFormat("Nothing to parse (empty string), measurementTime: {measurementTime}", model.MeasurementTime);
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
    }
}
