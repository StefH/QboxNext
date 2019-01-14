using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Qboxes.Classes;
using QboxNext.Core.Encryption;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using QBoxNext.Business.Interfaces.Internal;
using QBoxNext.Business.Interfaces.Public;
using QBoxNext.Business.Models;
using System;
using System.Collections.Generic;
using QboxNext.Common.Validation;

namespace QBoxNext.Business.Implementations
{
    internal class QboxDataDumpContextFactory : IQboxDataDumpContextFactory
    {
        private readonly ILogger<QboxDataDumpContextFactory> _logger;
        private readonly IStorageProviderFactory _storageProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="QboxDataDumpContextFactory"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The StorageProviderFactory.</param>
        /// <param name="logger">The logger.</param>
        public QboxDataDumpContextFactory([NotNull] IStorageProviderFactory storageProviderFactory, [NotNull] ILogger<QboxDataDumpContextFactory> logger)
        {
            Guard.NotNull(storageProviderFactory, nameof(storageProviderFactory));
            Guard.NotNull(logger, nameof(logger));

            _storageProviderFactory = storageProviderFactory;
            _logger = logger;
        }

        /// <inheritdoc cref="IQboxDataDumpContextFactory.Create"/>
        public QboxDataDumpContext Create(QboxContext context)
        {
            Guard.NotNull(context, nameof(context));

            try
            {
                string lastSeenAtUrl = context.LastSeenAtUrl;
                string externalIp = context.ExternalIp;
                var message = QboxMessageDecrypter.DecryptPlainOrEncryptedMessage(context.Message);
                int length = context.Message.Length;

                var mini = InitMini(context.SerialNumber, context.ProductNumber);

                return new QboxDataDumpContext(message, length, lastSeenAtUrl, externalIp, mini);
            }
            catch (Exception e)
            {
                string errorMessage = $"SerialNumber: {context.SerialNumber} ProductNumber: {context.ProductNumber} original error message: {e.Message}";
                _logger.LogError(e, errorMessage);

                return new QboxDataDumpContext("N/A", 0, "N/A", "N/A", null, e.Message + " - " + errorMessage); //refactor: beter oplossen wordt nu gecontroleerd in de controller en die gooit een exception
            }
        }

        private MiniPoco InitMini(string serialNumber, string productNumber)
        {
            var counterSensorMappingsSmartMeter = new CounterSensorMappingPoco
            {
                PeriodeBegin = new DateTime(2000, 1, 1),
                Formule = 1000
            };

            var mini = new MiniPoco
            {
                SerialNumber = serialNumber,
                Counters = new List<CounterPoco>
                {
                    new CounterPoco { CounterId = 181 },
                    new CounterPoco { CounterId = 182 },
                    new CounterPoco { CounterId = 281 },
                    new CounterPoco { CounterId = 282 },
                    new CounterPoco { CounterId = 2421 }
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

            // Init all Counters
            foreach (var counter in mini.Counters)
            {
                counter.QboxSerial = serialNumber;
                counter.CounterSensorMappings = new List<CounterSensorMappingPoco> { counterSensorMappingsSmartMeter };
                counter.StorageProvider = _storageProviderFactory.Create(serialNumber, productNumber, counter.CounterId);
            }

            return mini;
        }
    }
}