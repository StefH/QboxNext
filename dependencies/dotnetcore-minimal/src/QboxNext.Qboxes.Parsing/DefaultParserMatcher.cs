using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace QboxNext.Qboxes.Parsing
{
    /// <summary>
    /// The default parser matcher. The best matching parser returned has an equal or lower protocol version than the message protocol.
    /// </summary>
    internal class DefaultParserMatcher : IParserMatcher
    {
        private readonly ILogger<DefaultParserMatcher> _logger;
        private readonly IList<ParserInfo> _registeredParsers;

        public DefaultParserMatcher(IEnumerable<ParserInfo> registeredParsers, ILogger<DefaultParserMatcher> logger)
        {
            _registeredParsers = registeredParsers?.ToList() ?? throw new ArgumentNullException(nameof(registeredParsers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!_registeredParsers.Any())
            {
                throw new ArgumentException("At least one parser must be provided.", nameof(registeredParsers));
            }
        }

        /// <inheritdoc />
        public ParserInfo Match(string message)
        {
            if (!TryGetProtocol(message, out int protocolNr))
            {
                return null;
            }

            return _registeredParsers.Aggregate((current, next) =>
            {
                if (next.MaxProtocolVersion <= protocolNr && current.MaxProtocolVersion < next.MaxProtocolVersion)
                {
                    return next;
                }

                return current;
            });
        }

        private bool TryGetProtocol(string message, out int protocolNr)
        {
            protocolNr = int.MinValue;

            if (message?.Length >= 2 && int.TryParse(message.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out protocolNr))
            {
                return true;
            }

            _logger.LogError("Can't parse protocol from message " + message);
            return false;
        }
    }
}
