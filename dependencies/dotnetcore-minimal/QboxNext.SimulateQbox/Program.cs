// SimulateQbox simulates a Qbox attached to any meter.
// It will send immediately, and then after each minute, so it will not send on the minute boundary.
// It saves the sequence nr of the Qbox so it can be safely stopped and started.

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using QboxNext.ConsoleApp;
using QboxNext.Logging;

namespace QboxNext.SimulateQbox
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            // Make sure that the default parsing and formatting of numbers is using '.' as floating point.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            var parser = new Parser(settings =>
            {
                settings.IgnoreUnknownArguments = true;
                settings.CaseSensitive = false;
                settings.AutoHelp = true;
                settings.AutoVersion = false;
            });

            IHost host = new ConsoleHostBuilder(args)
                .ParseArguments<QboxSimulatorOptions>(parser)
                .ConfigureServices((context, services) => { services.AddHostedService<ConsoleRunner>(); })
                .ConfigureLogging((context, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddNLog();
                })
                // Listen for CTRL+C or SIGTERM.
                .UseConsoleLifetime()
                .Build();

            // Setup static logger factory.
            QboxNextLogProvider.LoggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            await host.RunWithArgumentsAsync<QboxSimulatorOptions>();
        }
    }
}