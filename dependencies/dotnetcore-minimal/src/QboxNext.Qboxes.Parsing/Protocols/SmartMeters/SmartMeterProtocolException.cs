using System;

namespace QboxNext.Qboxes.Parsing.Protocols.SmartMeters
{
    public class SmartMeterProtocolException : ProtocolException
    {
        public SmartMeterProtocolException(string message) : base(message)
        {
        }

        public SmartMeterProtocolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
