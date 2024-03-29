﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QboxNext.Core.Extensions;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Extensions.Models.Public;
using QboxNext.Logging;
using QboxNext.Server.Infrastructure.Azure.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.AzureTableImporter
{
    class Program
    {
        /// <summary>
        /// args[0] should be like 'C:\Users\***\Qbox\Qbox_xx-xx-xxx-xxx\xx-xx-xxx-xxx_0000'
        /// args[1] can be the connectionString
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string path = args[0];

            // Create ServiceCollection
            var services = new ServiceCollection();

            // Configure
            services.Configure<AzureTableStorageOptions>(options =>
            {
                options.ServerTimeout = 60;
                options.ConnectionString = args[1] ?? "UseDevelopmentStorage=true;";
                options.MeasurementsTableName = "QboxMeasurements";
                options.StatesTableName = "not-used";
                options.RegistrationsTableName = "not-used";
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

            // Setup static logger factory
            var factory = serviceProvider.GetService<ILoggerFactory>();
            QboxNextLogProvider.LoggerFactory = factory;

            // Resolve ILogger via DI
            var logger = factory.CreateLogger<Program>();
            logger.LogInformation("Start");

            // Resolve service via DI
            var service = serviceProvider.GetService<ICounterStoreService>();

            RunAsync(logger, service, path).GetAwaiter().GetResult();

            Console.WriteLine("Done");
        }

        private static async Task RunAsync(ILogger logger, ICounterStoreService service, string path)
        {
            int roundAtMinutes = 5;
            string sn = Path.GetFileNameWithoutExtension(path).Substring(0, 13);
            string[] counterIds = { "0181", "0182", "0281", "0282", "2421" };

            var list = new Dictionary<string, IList<CounterData>>();
            foreach (var counterId in counterIds)
            {
                var result = ReadFile(logger, path + counterId + ".qbx", sn, int.Parse(counterId));

                list.Add(counterId, result);
            }

            int batchValue = 12345;
            var values = list.Values.SelectMany(v => v).ToList();
            var f = values.First();
            var l = values.Last();
            logger.LogInformation($"First: {f} and Last = {l}");

            var grouped = from v in values
                          group v by new
                          {
                              v.SerialNumber,
                              v.CounterId,
                              MeasureTimeRounded = v.MeasureTime.Truncate(TimeSpan.FromMinutes(roundAtMinutes))
                          }
                into g
                          select new CounterData
                          {
                              SerialNumber = g.Key.SerialNumber,
                              CounterId = g.Key.CounterId,
                              MeasureTime = g.Key.MeasureTimeRounded,
                              PulseValue = g.Max(s => s.PulseValue)
                          };

            var sorted = grouped.OrderBy(k => k.MeasureTime).ToList();
            var sortedAndGrouped = sorted.GroupBy(s => s.MeasureTime).ToList();

            Console.WriteLine($"values           Count = {values.Count}");
            Console.WriteLine($"sorted           Count = {sorted.Count}");
            Console.WriteLine($"sortedAndGrouped Count = {sortedAndGrouped.Count}");

            int i = 1;
            var batch = new List<CounterData>();
            foreach (var grp in sortedAndGrouped)
            {
                string guid = Guid.NewGuid().ToString();
                foreach (var x in grp)
                {
                    if (batch.Count < batchValue)
                    {
                        batch.Add(x);
                    }
                    else
                    {
                        Console.WriteLine($"{(100.0 * i) / sortedAndGrouped.Count:F}%");

                        await service.StoreAsync(guid, batch);
                        batch.Clear();
                    }
                }

                i = i + 1;
            }
        }

        private static IList<CounterData> ReadFile(ILogger logger, string path, string sn, int counterId)
        {
            var result = new List<CounterData>();
            using (var reader = new BinaryReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                DateTime startOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1)).ToUniversalTime();
                DateTime endOfFile = DateTime.FromBinary(reader.ReadInt64()).Truncate(TimeSpan.FromMinutes(1)).ToUniversalTime();
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

                    if (raw < ulong.MaxValue) //  
                    {
                        //logger.LogInformation($"{currentTimestamp:yyyy-MM-dd HH:mm} | {raw,10}");

                        var counterData = new CounterData
                        {
                            SerialNumber = sn,
                            CounterId = counterId,
                            PulseValue = Convert.ToInt32(raw),
                            MeasureTime = currentTimestamp // .ToAmsterdam().ToLocalTime() // Change to Dutch Timezone
                        };
                        result.Add(counterData);
                    }

                    rawPrevious = raw;

                    currentTimestamp = currentTimestamp.AddMinutes(1);
                }
            }

            return result;
        }
    }
}