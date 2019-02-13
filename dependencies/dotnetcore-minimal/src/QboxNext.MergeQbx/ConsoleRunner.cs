using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QboxNext.MergeQbx.Utils;
using QboxNext.Storage;
using QboxNext.Storage.Qbx;

namespace QboxNext.MergeQbx
{
    public class ConsoleRunner : IHostedService
    {
        private readonly ILogger<ConsoleRunner> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly kWhStorageOptions _kWhStorageOptions;
        private readonly CommandLineOptions _options;

        public ConsoleRunner(ILoggerFactory loggerFactory, IOptions<CommandLineOptions> options, IOptions<kWhStorageOptions> kWhStorageOptions)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<ConsoleRunner>();
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _kWhStorageOptions = kWhStorageOptions?.Value ?? new kWhStorageOptions();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (!FileExists(_options.OriginalQbxPath) || !FileExists(_options.NewQbxPath))
            {
                return Task.CompletedTask;
            }

            using (kWhStorage originalStorageProvider = GetStorageProviderForPath(_options.OriginalQbxPath))
            {
                using (kWhStorage newStorageProvider = GetStorageProviderForPath(_options.NewQbxPath))
                {
                    originalStorageProvider.Merge(newStorageProvider);
                }
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private bool FileExists(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }

            _logger.LogError($"Could not find file {path}");
            Environment.ExitCode = -1;
            return false;

        }

        private kWhStorage GetStorageProviderForPath(string originalQbxPath)
        {
            var storageProviderContext = new StorageProviderContext
            {
                SerialNumber = QbxPathUtils.GetSerialFromPath(originalQbxPath),
                CounterId = QbxPathUtils.GetCounterIdFromPath(originalQbxPath),
                Precision = Precision.mWh,
                StorageId = QbxPathUtils.GetStorageIdFromPath(originalQbxPath)
            };

            IOptions<kWhStorageOptions> options = new OptionsWrapper<kWhStorageOptions>(_kWhStorageOptions);

            // Override path.
            options.Value.DataStorePath = QbxPathUtils.GetBaseDirFromPath(originalQbxPath);

            return new kWhStorage(_loggerFactory, options, storageProviderContext);
        }

    }
}