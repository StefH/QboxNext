using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QboxNext.ConsoleApp;
using QboxNext.ConsoleApp.Loggers;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Extensions;

namespace QboxNext.ParseQboxMessage
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var parser = new Parser(settings =>
            {
                settings.IgnoreUnknownArguments = true;
                settings.CaseSensitive = false;
                settings.AutoHelp = true;
            });

            IHost host = new ConsoleHostBuilder(args)
                .ParseArguments<CommandLineOptions>(parser)
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddParsers()
                        .AddHostedService<ConsoleRunner>();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddSimpleConsole();
                })
                // Listen for CTRL+C or SIGTERM.
                .UseConsoleLifetime()
                .Build();

            // Setup static logger factory.
            QboxNextLogProvider.LoggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            using (host)
            {
                await host.StartWithArgumentsAsync<CommandLineOptions>();
            }
        }
    }
}