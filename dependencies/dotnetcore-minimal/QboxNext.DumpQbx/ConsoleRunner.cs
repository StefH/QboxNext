using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QboxNext.Core.Extensions;

namespace QboxNext.DumpQbx
{
    public class ConsoleRunner : IHostedService
    {
        private readonly ILogger<ConsoleRunner> _logger;
        private readonly CommandLineOptions _options;
        private readonly IApplicationLifetime _applicationLifetime;

        public ConsoleRunner(ILogger<ConsoleRunner> logger, IOptions<CommandLineOptions> options, IApplicationLifetime applicationLifetime)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            using (var reader = new BinaryReader(File.Open(_options.QbxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                DateTime startOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                DateTime endOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                var id = new Guid(reader.ReadBytes(16));

                _logger.LogInformation("StartOfFile: {0}", startOfFile);
                _logger.LogInformation("EndOfFile:   {0}", endOfFile);
                _logger.LogInformation("ID:          {0}", id);

                if (_options.IsDumpingValues)
                {
                    _logger.LogInformation("Timestamp NL     : {0,10}, {1,10}, {2,10}, {3,5} (kWh can be kWh, Wh or mWh depending on Precision setting)", "raw", "kWh", "money", "quality");

                    long length = reader.BaseStream.Length;
                    DateTime currentTimestamp = startOfFile;
                    while (reader.BaseStream.Position < length)
                    {
                        if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
                        {
                            break;
                        }

                        ulong raw = reader.ReadUInt64();
                        ulong kWh = reader.ReadUInt64();
                        ulong money = reader.ReadUInt64();
                        ushort quality = reader.ReadUInt16();

                        string logLine = currentTimestamp.ToString("yyyy-MM-dd HH:mm : ");
                        if (raw == ulong.MaxValue && kWh == 0 && money == 0 && quality == 0)
                        {
                            _logger.LogInformation(logLine + "empty slot");
                        }
                        else
                        {
                            string rawDisplay = raw == ulong.MaxValue ? "<none>" : raw.ToString(CultureInfo.InvariantCulture);
                            _logger.LogInformation(logLine + "{0,10}, {1,10}, {2,10}, {3,5}", rawDisplay, kWh, money, quality);
                        }

                        currentTimestamp = currentTimestamp.AddMinutes(1);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}