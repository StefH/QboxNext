using System;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// An exception that is thrown by the protocol reader.
    /// </summary>
    public class ProtocolReaderException : ProtocolException
    {
        private readonly ReadOnlyMemory<char> _stream;

        public ProtocolReaderException(string message, ReadOnlyMemory<char> stream, int position) : base(message)
        {
            _stream = stream;
            Position = position;
        }

        public ProtocolReaderException(string message, ReadOnlyMemory<char> stream, int position, Exception innerException) : base(message, innerException)
        {
            _stream = stream;
            Position = position;
        }

        public string Buffer => _stream.ToString();

        public int Position { get; }

        public int Length => _stream.Length;
    }
}
