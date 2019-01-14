using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using QboxNext.Core.Encryption;
using QboxNext.Core.Utils;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using QBoxNext.Business.Interfaces.Public;
using QBoxNext.Business.Models;
using System;
using System.Collections.Generic;

namespace QBoxNext.Business.Implementations
{
    public class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private readonly ILogger<QboxDataDumpContextFactory> _logger;
        private readonly IStorageProvider _storageProvider;

        //public QboxDataDumpContextFactory([NotNull] IStorageProvider storageProvider, [NotNull] ILogger<QboxDataDumpContextFactory> logger)
        //{
        //    Guard.IsNotNull(logger, nameof(logger));

        //    _storageProvider = storageProvider;
        //    _logger = logger;
        //}

        public QboxDataDumpContextFactory([NotNull] ILogger<QboxDataDumpContextFactory> logger)
        {
            Guard.IsNotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <inheritdoc cref="IQboxDataDumpContextFactory.CreateContext(QboxContext)"/>
        public QboxDataDumpContext CreateContext(QboxContext context)
        {
            Guard.IsNotNull(context, nameof(context));

            try
            {
                string lastSeenAtUrl = context.LastSeenAtUrl;
                string externalIp = context.ExternalIp;
                var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(context.Message);
                int length = context.Message.Length;

                var mini = InitMini(context.SerialNumber);

                return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini);
            }
            catch (Exception e)
            {
                string s = $"SerialNumber: {context.SerialNumber} ProductNumber: {context.ProductNumber} original error message: {e.Message}";
                _logger.LogError(e, s);
                return new QboxDataDumpContext("N/A", 0, "N/A", "N/A", null, error: e.Message + " - " + s); //refactor: beter oplossen wordt nu gecontroleerd in de controller en die gooit een exception
            }
            finally
            {
                _logger.LogTrace("Return");
            }
        }

        /// <summary>
		/// Returns a mini poco by serial number. First tries the Redis cache repository and if not found
		/// checks if it can find the box in Eco.
		/// Upon connection exception it will also fall back to ECO.
		/// </summary>
		/// <param name="sn">Serialnumber of the Mini</param>
		/// <returns>MiniPoco object holding the Mini data</returns>
        private MiniPoco InitMini(string sn)
        {
            try
            {
                var counterSensorMappingsSmartMeter = new CounterSensorMappingPoco
                {
                    PeriodeBegin = new DateTime(2000, 1, 1),
                    Formule = 1000
                };

                var mini = new MiniPoco
                {
                    SerialNumber = sn,
                    DataStorePath = Environment.OSVersion.Platform == PlatformID.Win32NT ? @"D:\QboxNextData" : "/var/qboxnextdata",
                    Counters = new List<CounterPoco>
                    {
                        new CounterPoco
                        {
                            CounterId = 181,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 182,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 281,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 282,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 2421,
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        },
                        new CounterPoco
                        {
                            CounterId = 1,
                            // This is not correct, since the Eltako's have different formula's. Keep it simple for now.
                            CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter }
                        }
                    },
                    Clients = new List<ClientQboxPoco>
                    {
                        new ClientQboxPoco
                        {
                            ClientId = 0,
                            MeterType = DeviceMeterType.Smart_Meter_EG,     // Main meter type for second Qbox of Duo.
                            SecondaryMeterType = DeviceMeterType.SO_Pulse   // Should be DeviceMeterType.SO_Pulse for Mono with Qbox Solar.
                        }
                    },
                    Precision = Precision.mWh,
                    MeterType = DeviceMeterType.NO_Meter,       // This should contain the actual meter type for Mono.
                    SecondaryMeterType = DeviceMeterType.None,  // This should be DeviceMeterType.SO_Pulse for Mono with Qbox Solar.
                    AutoAnswer = true
                };

                mini.PrepareCounters();

                // TODO _storageProvider
                mini.SetStorageProvider();

                return mini;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }

            throw new ArgumentOutOfRangeException(nameof(sn));
        }
    }
}