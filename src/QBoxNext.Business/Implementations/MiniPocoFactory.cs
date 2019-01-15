using JetBrains.Annotations;
using QboxNext.Common.Validation;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using QBoxNext.Business.Interfaces.Internal;
using System;
using System.Collections.Generic;

namespace QBoxNext.Business.Implementations
{
    internal class MiniPocoFactory : IMiniPocoFactory
    {
        private readonly IStorageProviderFactory _storageProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniPocoFactory"/> class.
        /// </summary>
        /// <param name="storageProviderFactory">The storage provider factory.</param>
        public MiniPocoFactory([NotNull] IStorageProviderFactory storageProviderFactory)
        {
            Guard.NotNull(storageProviderFactory, nameof(storageProviderFactory));

            _storageProviderFactory = storageProviderFactory;
        }

        /// <inheritdoc cref="IMiniPocoFactory.Create(string, string)"/>
        public MiniPoco Create(string serialNumber, string productNumber)
        {
            Guard.NotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.NotNullOrEmpty(productNumber, nameof(productNumber));

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