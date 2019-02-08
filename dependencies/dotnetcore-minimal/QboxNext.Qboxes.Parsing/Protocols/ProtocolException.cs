using System;
using System.IO;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// An exception that is thrown when parsing/processing the message protocol.
    /// </summary>
    public class ProtocolException : IOException
    {
        public ProtocolException(string message) : base(message)
        {
        }

        public ProtocolException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}