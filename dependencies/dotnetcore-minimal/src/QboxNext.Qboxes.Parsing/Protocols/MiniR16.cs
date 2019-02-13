using System;
using Microsoft.Extensions.Logging;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// MiniR16, Firmware 39 (0x27)
    /// - ClientID added to client settings and client info DeviceSettings
    /// - GroupID added to counterpayloads
    /// </summary>
    public class MiniR16 : MiniR07
    {
        private readonly ILogger<MiniR16> _logger;

        public MiniR16(ILogger<MiniR16> logger, IProtocolReaderFactory protocolReaderFactory, SmartMeterCounterParser smartMeterCounterParser) : base(logger, protocolReaderFactory, smartMeterCounterParser)
        {
            _logger = logger;
        }

        protected virtual void AddCounterPayload(Mini07ParseModel model, CounterGroup group)
        {
            model.Payloads.Add(new CounterWithSourcePayload
            {
                InternalNr = Reader.ReadByte(),
                Value = Reader.ReadUInt32(),
                PrimaryMeter = group.PrimaryMeterCounters,
                Source = group.CounterSource
            });
        }

        protected override void ParseCounters(Mini07ParseModel model)
        {
            _logger.LogDebug("Nr of countergroups: {nrOfCounters}", model.PayloadIndicator.NrOfCounters);
            int i = 0;
            // Total number of counters
            while (i < model.PayloadIndicator.NrOfCounters)
            {
                try
                {
                    // Grouped per CounterGroup
                    var group = new CounterGroup(Reader.ReadByte());
                    for (var c = 0; c < group.NbrOfCounters; c++)
                    {
                        AddCounterPayload(model, group);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("Parsing counter payload (payload {0} of {1})", i + 1, model.PayloadIndicator.NrOfCounters), ex);
                }
            }
        }
    }
}