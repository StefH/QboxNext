using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QboxNext.Core.Utils;
using QboxNext.Qboxes.Parsing;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.ParseQboxMessage
{
    public class ConsoleRunner : IHostedService
    {
        private readonly ILogger<ConsoleRunner> _logger;
        private readonly CommandLineOptions _options;
        private readonly IParserFactory _parserFactory;

        public ConsoleRunner(ILogger<ConsoleRunner> logger, IOptions<CommandLineOptions> options, IParserFactory parserFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(ParseString(_options.Message));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private string ParseString(string message)
        {
            message = message.WithoutStxEtxEnvelope().Trim();
            IMessageParser parser = _parserFactory.GetParser(message);
            BaseParseResult result = parser.Parse(message);
            // todo: evaluate
            // It's not possible to determine if hex is a request or response value, check is now based on ErrorParseResult
            if (result is ErrorParseResult)
            {
                parser = _parserFactory.GetParser(typeof(MiniResponse));
                result = parser.Parse(message);
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
        }
    }
}