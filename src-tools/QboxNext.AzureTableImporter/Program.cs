using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Infrastructure.Azure.Options;
using QboxNext.Logging;
using QboxNext.Qserver.Core.Utils;
using System;
using System.IO;

namespace QboxNext.AzureTableImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create ServiceCollection
            var services = new ServiceCollection();

            // Configure
            services.Configure<AzureTableStorageOptions>(options =>
            {
                options.ServerTimeout = 60;
                options.ConnectionString = "UseDevelopmentStorage=true;";
                options.MeasurementsTableName = "QboxMeasurementsImport";
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

            Run(logger, service, args[0]);
        }

        private static void Run(ILogger logger, ICounterStoreService service, string path)
        {
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                DateTime startOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                DateTime endOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1));
                Guid id = new Guid(reader.ReadBytes(16));

                logger.LogInformation("StartOfFile: {0}", startOfFile);
                logger.LogInformation("EndOfFile:   {0}", endOfFile);
                logger.LogInformation("ID:          {0}", id);

                long length = reader.BaseStream.Length;
                DateTime currentTimestamp = startOfFile;
                while (reader.BaseStream.Position < length)
                {
                    ulong raw = reader.ReadUInt64();
                    ulong kWh = reader.ReadUInt64();
                    ulong money = reader.ReadUInt64();
                    int quality = reader.ReadUInt16();

                    //System.Console.Write(currentTimestamp.ToString("yyyy-MM-dd HH:mm : "));
                    if (raw == ulong.MaxValue && kWh == 0 && money == 0 && quality == 0)
                    {
                        // System.Console.WriteLine("empty slot");
                    }
                    else
                    {
                        if (raw < ulong.MaxValue)
                        {
                            logger.LogInformation($"{currentTimestamp:yyyy-MM-dd HH:mm} | {raw,10} | {kWh,10} | {money,10} | {quality,5}");
                        }
                        //var rawDisplay = raw == ulong.MaxValue ? "<none>" : raw.ToString(CultureInfo.InvariantCulture);
                        //System.Console.WriteLine("{0,10}, {1,10}, {2,10}, {3,5}", rawDisplay, kWh, money, quality);
                    }

                    currentTimestamp = currentTimestamp.AddMinutes(1);
                }
            }
        }
    }
}