using System;
using System.Collections.Generic;
using QboxNext.Core.Dto;
using QboxNext.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;
using QboxNext.Qserver.Core.Interfaces;
using QboxNext.Qserver.Core.Model;

namespace QboxNext.Model.Utils
{
    /// <summary>
    /// Utility class to incrementally build a Qbox (Mini) object.
    /// </summary>
    public class QboxBuilder
    {
        private class CounterSpec
        {
            public int CounterId { get; set; }
            public CounterType CounterType { get; set; }
            public DeviceEnergyType DeviceEnergyType { get; set; }
        }

        private string _qboxSerial;

        private QboxType _qboxType = QboxType.Duo;

        private List<CounterSpec> _counters = new List<CounterSpec>();

        private static readonly CounterSensorMapping CounterSensorMappingsSmartMeter = new CounterSensorMapping
        {
            PeriodeBegin = new DateTime(2012, 1, 1),
            Formule = 1000
        };

        /// <summary>
        /// Set the type of the Qbox to build.
        /// </summary>
        public QboxBuilder WithType(QboxType qboxType)
        {
            _qboxType = qboxType;
            return this;
        }

        /// <summary>
        /// Set the serial number of the Qbox to build.
        /// </summary>
        /// <param name="qboxSerial">Serial number of the Qbox.</param>
        public QboxBuilder WithSerial(string qboxSerial)
        {
            _qboxSerial = qboxSerial;
            return this;
        }

        /// <summary>
        /// Add a counter to the Qbox to build.
        /// </summary>
        /// <param name="counterId">ID of the counter</param>
        /// <param name="deviceEnergyType">Type of the device (graph line)</param>
        public QboxBuilder WithCounter(int counterId, CounterType counterType, DeviceEnergyType deviceEnergyType)
        {
            _counters.Add(new CounterSpec
            {
                CounterId = counterId,
                CounterType = counterType,
                DeviceEnergyType = deviceEnergyType
            });
            return this;
        }

        /// <summary>
        /// Create the Qbox object from the configured properties.
        /// </summary>
        public Mini Build()
        {
            VerifyConfiguration();
            Qbox qbox = CreateQbox(_qboxSerial);
            Mini mini = CreateMini(_qboxSerial);
            foreach (CounterSpec counterSpec in _counters)
            {
                mini.Counters.Add(CreateCounter(counterSpec, qbox));
            }
            return mini;
        }

        private void VerifyConfiguration()
        {
            if (string.IsNullOrEmpty(_qboxSerial))
            {
                throw new QboxBuilderException("Qbox serial number not specified. Use .WithSerial on the builder to specify a Qbox serial numbers.");
            }
            if (_counters.Count == 0)
            {
                throw new QboxBuilderException("No counters were specified. Use .WithCounter on the builder to add counters.");
            }
            foreach (CounterSpec counterSpec in _counters)
            {
                if (!IsAllowedCounterId(counterSpec.CounterId))
                {
                    throw new QboxBuilderException($"Invalid counter ID {counterSpec.CounterId}");
                }
            }
        }

        private bool IsAllowedCounterId(int counterId)
        {
            switch (counterId)
            {
                case 1:
                case 3:
                case 181:
                case 182:
                case 281:
                case 282:
                case 2421:
                    return true;
                default:
                    return false;
            }
        }

        private Qbox CreateQbox(string qboxSerial)
        {
            return new Qbox
            {
                SerialNumber = qboxSerial,
                Precision = Precision.mWh,
                DataStore = new DataStore
                {
                    Path = QboxNext.Core.Config.DataStorePath
                }
            };
        }

        private Mini CreateMini(string qboxSerial)
        {
            var mini = new Mini()
            {
                SerialNumber = qboxSerial,
                DataStorePath = QboxNext.Core.Config.DataStorePath,
                Precision = Precision.mWh,
                AutoAnswer = true,
                MeterType = _qboxType == QboxType.Mono ? DeviceMeterType.Smart_Meter_EG : DeviceMeterType.NO_Meter,
                SecondaryMeterType = _qboxType == QboxType.Mono ? DeviceMeterType.SO_Pulse : DeviceMeterType.NO_Meter,
            };

            if (_qboxType == QboxType.Duo)
            {
                mini.Clients.Add(new ClientQbox
                {
                    ClientId = 0,
                    MeterType = DeviceMeterType.Smart_Meter_EG,
                    SecondaryMeterType = DeviceMeterType.SO_Pulse
                });
            }

            return mini;
        }

        private Counter CreateCounter(CounterSpec counterSpec, Qbox qbox)
        {
            // NOTE: Using CounterSensorMappingsSmartMeter is not correct for S0 (generation), 
            // since the Eltakos have different formulas. Keep it simple for now.
            return new Counter
            {
                CounterId = counterSpec.CounterId,
                Secondary = counterSpec.CounterType == CounterType.Secondary,
                GroupId = _qboxType == QboxType.Mono ? CounterSource.Host : CounterSource.Client0,
                CounterSensorMappings = new List<CounterSensorMapping> { CounterSensorMappingsSmartMeter },
                CounterDeviceMappings = new List<CounterDeviceMapping>
                    {
                        new CounterDeviceMapping
                        {
                            Device = new Device
                            {
                                EnergyType = counterSpec.DeviceEnergyType
                            },
                            PeriodeBegin = new DateTime(2012, 1, 1),
                            PeriodeEind = new DateTime(9999, 1, 1)
                        }
                    },
                Qbox = qbox
            };
        }
    }
}
