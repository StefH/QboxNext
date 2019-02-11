using System;
using Microsoft.Extensions.DependencyInjection;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing
{
    internal class DefaultParserFactory : IParserFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IParserMatcher _parserMatcher;

        public DefaultParserFactory(IServiceProvider serviceProvider, IParserMatcher parserMatcher)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _parserMatcher = parserMatcher ?? throw new ArgumentNullException(nameof(parserMatcher));
        }

        /// <summary>
        /// Gets a parser by extracting the protocol version from the message.
        /// </summary>
        /// <param name="message">The message to extract the protocol version of.</param>
        /// <returns>Returns a message parser.</returns>
        /// <remarks>
        /// First 2 bytes (= protocol version).
        /// </remarks>
        public IMessageParser GetParser(string message)
        {
            ParserInfo parserInfo = _parserMatcher.Match(message);
            if (parserInfo == null)
            {
                throw new InvalidOperationException("No parser found for message " + message);
            }

            return GetParser(parserInfo.Type);
        }
        
        public IMessageParser GetParser(Type parserType)
        {
            return _serviceProvider.GetRequiredService(parserType) as IMessageParser;
        }
    }
}
