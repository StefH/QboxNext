using JetBrains.Annotations;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Internal;
using QboxNext.Extensions.Interfaces.Public;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;
using System.Collections.Generic;

namespace QboxNext.Extensions.Implementations
{
    internal class MiniPocoFactory : IMiniPocoFactory
    {
        private readonly IAsyncStorageProviderFactory _asyncStorageProviderFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniPocoFactory"/> class.
        /// </summary>
        /// <param name="asyncStorageProviderFactory">The storage provider factory.</param>
        public MiniPocoFactory([NotNull] IAsyncStorageProviderFactory asyncStorageProviderFactory)
        {
            Guard.IsNotNull(asyncStorageProviderFactory, nameof(asyncStorageProviderFactory));

            _asyncStorageProviderFactory = asyncStorageProviderFactory;
        }

        /// <inheritdoc cref="IMiniPocoFactory.Create(string, string)"/>
        public MiniPoco Create(string serialNumber, string productNumber)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));
            Guard.IsNotNullOrEmpty(productNumber, nameof(productNumber));

            var counterSensorMappingsSmartMeter = new CounterSensorMappingPoco { Formule = 1000 /* inPulsesPerUnit */ };

            var mini = new MiniPoco
            {
                Id = productNumber, // TODO : HACK
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
                counter.StorageProvider = _asyncStorageProviderFactory.Create(serialNumber, productNumber, counter.CounterId);
            }

            return mini;
        }
    }
}