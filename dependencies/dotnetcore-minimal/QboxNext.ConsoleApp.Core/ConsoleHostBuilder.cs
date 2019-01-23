using System;
using System.IO;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QboxNext.ConsoleApp
{
    /// <summary>
    /// Represents a <see cref="IHost" /> builder for shared console apps, utilizing the common setup we use.
    /// </summary>
    public class ConsoleHostBuilder : HostBuilder
    {
        private readonly string[] _args;

        public ConsoleHostBuilder(string[] args)
        {
            _args = args;

            this.UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureHostConfiguration(config =>
                {
                    config
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddEnvironmentVariables("ASPNETCORE_")
                        // ReSharper disable once StringLiteralTypo
                        // ReSharper disable once ArgumentsStyleLiteral
                        .AddJsonFile("hostsettings.json", optional: true);

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    IHostingEnvironment env = context.HostingEnvironment;
                    config.AddEnvironmentVariables();

                    // ReSharper disable StringLiteralTypo
                    // ReSharper disable ArgumentsStyleLiteral
                    config
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    // ReSharper restore ArgumentsStyleLiteral
                    // ReSharper restore StringLiteralTypo
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
                    services.AddLogging();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddDebug();
                    }
                });
        }

        /// <summary>
        /// Use provided command line parser to parse the command line arguments. Use in conjunction with <see cref="HostExtensions.RunWithArgumentsAsync{TCommandLineOptions}" />.
        /// </summary>
        /// <typeparam name="TCommandLineOptions">The command line options to add.</typeparam>
        /// <param name="parser">The parser to parse the command line argument values with.</param>
        /// <returns>The builder instance.</returns>
        public ConsoleHostBuilder ParseArguments<TCommandLineOptions>(Parser parser)
            where TCommandLineOptions : class, new()
        {
            ConfigureServices((context, services) =>
            {
                ParserResult<TCommandLineOptions> parserResult = parser
                    .ParseArguments<TCommandLineOptions>(_args)
                    .WithParsed(options =>
                    {
                        // Store parsed arguments in container as IOptions<>.
                        services.AddOptions();
                        services.AddSingleton<IOptions<TCommandLineOptions>>(new OptionsWrapper<TCommandLineOptions>(options));
                    });

                // Store results in container.
                services.AddSingleton(parserResult);
            });
            return this;
        }
    }
}