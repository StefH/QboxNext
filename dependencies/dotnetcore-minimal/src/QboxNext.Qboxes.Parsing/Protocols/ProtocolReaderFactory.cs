using System;
using Microsoft.Extensions.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
    /// <summary>
    /// A factory for creating <see cref="ProtocolReader"/>s.
    /// </summary>
    internal class ProtocolReaderFactory : IProtocolReaderFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolReaderFactory"/> class.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public ProtocolReaderFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <inheritdoc />
        public IProtocolReader Create(ReadOnlyMemory<char> memory)
        {
            return new ProtocolReader(_loggerFactory.CreateLogger<ProtocolReader>(), memory);
        }
    }
}