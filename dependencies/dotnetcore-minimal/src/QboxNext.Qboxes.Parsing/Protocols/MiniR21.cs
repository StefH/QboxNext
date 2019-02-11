using Microsoft.Extensions.Logging;
using QboxNext.Qboxes.Parsing.Protocols.SmartMeters;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    public class MiniR21 : MiniR16
    {
        public MiniR21(ILogger<MiniR21> logger, IProtocolReaderFactory protocolReaderFactory, SmartMeterCounterParser smartMeterCounterParser) : base(logger, protocolReaderFactory, smartMeterCounterParser)
        {
        }

        protected override void AddCounterPayload(Mini07ParseModel model, CounterGroup group)
        {
            byte number = Reader.ReadByte();
            model.Payloads.Add(new R21CounterPayload
            {
                InternalNr = number & 0x7F,
                // Bit 7 (0x80) indicates if the counter value is valid (0) / invalid (1)
                IsValid = (number & 0x80) == 0x00,
                Value = Reader.ReadUInt32(),
                PrimaryMeter = group.PrimaryMeterCounters,
                Source = group.CounterSource
            });
        }

        /// <summary>
        /// ClientID Added to Client Status
        /// New Client Info fields (0x45 .. 0x4F)
        /// New Client Activity request (0xA8 .. 0xAA)
        /// </summary>
        public const int ProtocolVersion = 41;
    }
}
