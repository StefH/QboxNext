using System;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;

namespace QboxNext.ConsoleApp
{
    public static class HostExtensions
    {
        /// <summary>
        /// Runs an application while handling the command line parsing/usage/help text and returns a Task that only completes when the token is triggered or shutdown is triggered.
        /// </summary>
        /// <typeparam name="TCommandLineOptions">The command line options type.</typeparam>
        /// <param name="host">The host.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task RunWithArgumentsAsync<TCommandLineOptions>(this IHost host, CancellationToken cancellationToken = default(CancellationToken))
            where TCommandLineOptions : class, new()
        {
            return ExecWithArgumentsAsync<TCommandLineOptions>(host, () => host.RunAsync(cancellationToken));
        }

        /// <summary>
        /// Starts the program while handling the command line parsing/usage/help text.
        /// </summary>
        /// <typeparam name="TCommandLineOptions">The command line options type.</typeparam>
        /// <param name="host">The host.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task StartWithArgumentsAsync<TCommandLineOptions>(this IHost host, CancellationToken cancellationToken = default(CancellationToken))
            where TCommandLineOptions : class, new()
        {
            return ExecWithArgumentsAsync<TCommandLineOptions>(host, () => host.StartAsync(cancellationToken));
        }

        /// <summary>
        /// Runs the host while handling the command line parsing/usage/help text.
        /// </summary>
        /// <typeparam name="TCommandLineOptions">The command line options type.</typeparam>
        /// <param name="host">The host.</param>
        /// <param name="executor">The task to execute.</param>
        private static async Task ExecWithArgumentsAsync<TCommandLineOptions>(this IHost host, Func<Task> executor)
            where TCommandLineOptions : class, new()
        {
            ILoggerFactory loggerFactory = QboxNextLogProvider.LoggerFactory = host.Services.GetRequiredService<ILoggerFactory>();

            var parserResult = host.Services.GetService<ParserResult<TCommandLineOptions>>();
            if (parserResult == null)
            {
                throw new InvalidOperationException("Configure the host with the command line parser extension ParseArguments().");
            }

            var env = host.Services.GetRequiredService<IHostingEnvironment>();
            ILogger logger = loggerFactory.CreateLogger(env.ApplicationName ?? "StartUp");

            if (parserResult.Tag == ParserResultType.Parsed)
            {
                try
                {
                    await executor().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An unhandled exception occurred.");
                }
            }
            else
            {
                LogHelpText(parserResult, logger);
            }
        }

        private static void LogHelpText<TCommandLineOptions>(ParserResult<TCommandLineOptions> parserResult, ILogger logger)
            where TCommandLineOptions : class, new()
        {
            var customHelpText = new TCommandLineOptions() as ICustomHelpText;

            string defaultHelpText = HelpText.AutoBuild(parserResult, int.MaxValue).ToString();
            string helpText = customHelpText?.GetHelpText(defaultHelpText) ?? defaultHelpText;
            logger.LogInformation(helpText);
        }
    }
}