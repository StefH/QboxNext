
using System;

namespace QboxNext.Qboxes.Parsing.Exceptions
{
    /// <summary>
    /// Base class for all exceptions related to the QboxNext parser classes
    /// </summary>
    [Serializable]
    public class BaseParserException : System.Exception
    {
        /// <summary>
        /// Constructor for the exception class
        /// </summary>
        public BaseParserException() { }
        public BaseParserException(string message): base(message) { }
    }

    [Serializable]
    public class SmartMeterMessageException : BaseParserException
    {
        public SmartMeterMessageException(string message) :
            base(message)
        {
        }
    }

    [Serializable]
    public class SmartMeterParseValueOutOfRange : SmartMeterMessageException
    {
        public SmartMeterParseValueOutOfRange(string message)
            : base(message)
        {
        }
    }

}
