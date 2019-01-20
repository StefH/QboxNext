using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QboxNext.Core.CommandLine;
using QboxNext.Core.Utils;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing;
using QboxNext.Qboxes.Parsing.Extensions;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.ParseQboxMessage
{
    class Program
    {
        [Option("m", "message", Required = true, HelpText = "Qbox message")]
        public string Message { get; set; }

        public static IServiceProvider ApplicationServiceProvider { get; private set; }

        static void Main(string[] args)
        {
            // Setup static logger factory
            ILoggerFactory loggerFactory = QboxNextLogProvider.LoggerFactory = new LoggerFactory();

            ApplicationServiceProvider = new ServiceCollection()
                .AddSingleton(loggerFactory)
                .AddLogging()
                .AddParsers()
                .BuildServiceProvider();

            var program = new Program();
            var settings = new CommandLineParserSettings { IgnoreUnknownArguments = true, CaseSensitive = false };
            ICommandLineParser parser = new CommandLineParser(settings);
            if (parser.ParseArguments(args, program, System.Console.Error))
            {
                program.Run();
            }
            else
            {
                System.Console.WriteLine("Usage: QboxNext.ParseQboxMessage --message=<message>");
            }
        }

        private void Run()
        {
            System.Console.WriteLine(ParseString(Message));
        }

        private string ParseString(string inMessage)
        {
            var parserFactory = ApplicationServiceProvider.GetRequiredService<IParserFactory>();
            inMessage = inMessage.WithoutStxEtxEnvelope().Trim();
            var parser = parserFactory.GetParser(inMessage);
            var result = parser.Parse(inMessage) as BaseParseResult;
            // todo: evaluate
            // It's not possible to determine if hex is a request or response value, check is now based on ErrorParseResult
            if (result is ErrorParseResult)
            {
                parser = ApplicationServiceProvider.GetRequiredService<MiniResponse>();
                result = parser.Parse(inMessage) as BaseParseResult;
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented, new Newtonsoft.Json.Converters.StringEnumConverter());
        }
    }
}
