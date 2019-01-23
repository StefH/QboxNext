using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QboxNext.MergeQbx.Utils;
using QboxNext.Qserver.Core.DataStore;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.MergeQbx
{
    public class ConsoleRunner : IHostedService
    {
        private readonly ILogger<ConsoleRunner> _logger;
        private readonly CommandLineOptions _options;

        public ConsoleRunner(ILogger<ConsoleRunner> logger, IOptions<CommandLineOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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

            _logger.LogError($"Could not find file {_options.OriginalQbxPath}");
            Environment.ExitCode = -1;
            return false;

        }

        private static kWhStorage GetStorageProviderForPath(string originalQbxPath)
        {
            string serial = QbxPathUtils.GetSerialFromPath(originalQbxPath);
            string baseDir = QbxPathUtils.GetBaseDirFromPath(originalQbxPath);
            int counterId = QbxPathUtils.GetCounterIdFromPath(originalQbxPath);
            string storageId = QbxPathUtils.GetStorageIdFromPath(originalQbxPath);
            return new kWhStorage(serial, baseDir, counterId, Precision.mWh, storageId);
        }
    }
}