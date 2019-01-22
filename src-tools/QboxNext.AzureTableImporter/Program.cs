using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Logging;
using QboxNext.Qserver.Core.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using QboxNext.Server.Infrastructure.Azure.Options;
using QBoxNext.Server.Business.DependencyInjection;

namespace QboxNext.AzureTableImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];

            // Create ServiceCollection
            var services = new ServiceCollection();

            // Configure
            services.Configure<AzureTableStorageOptions>(options =>
            {
                options.ServerTimeout = 60;
                options.ConnectionString = "UseDevelopmentStorage=true;";
                options.MeasurementsTableName = "QboxMeasurementsImport"; // + int.Parse(Path.GetFileNameWithoutExtension(path).Substring(14));
                options.StatesTableName = "QboxStatesNotUsed";
            });


            // Add logging & services
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
                builder.AddDebug();
            });
            services.AddBusiness();

            var serviceProvider = services.BuildServiceProvider();

            // Resolve ILoggerFactory and ILogger via DI

            // Setup static logger factory
            var factory = serviceProvider.GetService<ILoggerFactory>();
            QboxNextLogProvider.LoggerFactory = factory;

            // Resolve ILogger via DI
            var logger = factory.CreateLogger<Program>();
            logger.LogInformation("Start");

            // Resolve service via DI
            var service = serviceProvider.GetService<ICounterStoreService>();

            RunAsync(logger, service, path).GetAwaiter().GetResult();
        }

        private static async Task RunAsync(ILogger logger, ICounterStoreService service, string path)
        {
            string sn = Path.GetFileNameWithoutExtension(path).Substring(0, 13);
            int counterId = int.Parse(Path.GetFileNameWithoutExtension(path).Substring(14));

            var x = new CounterData
            {
                SerialNumber = sn,
                CounterId = counterId
            };

            using (var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                DateTime startOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                DateTime endOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                Guid id = new Guid(reader.ReadBytes(16));

                logger.LogInformation("StartOfFile: {0}", startOfFile);
                logger.LogInformation("EndOfFile:   {0}", endOfFile);
                logger.LogInformation("ID:          {0}", id);

                ulong rawPrevious = 0;
                long length = reader.BaseStream.Length;
                DateTime currentTimestamp = startOfFile;
                while (reader.BaseStream.Position < length)
                {
                    ulong raw = reader.ReadUInt64();
                    ulong kWh = reader.ReadUInt64();
                    ulong money = reader.ReadUInt64();
                    int quality = reader.ReadUInt16();

                    if (raw != rawPrevious && raw < ulong.MaxValue)
                    {
                        logger.LogInformation($"{currentTimestamp:yyyy-MM-dd HH:mm} | {raw,10}");

                        x.PulseValue = raw;
                        x.MeasureTime = currentTimestamp;
                        await service.StoreAsync(Guid.NewGuid().ToString(), x);
                    }

                    rawPrevious = raw;

                    currentTimestamp = currentTimestamp.AddMinutes(1);
                }
            }
        }
    }
}