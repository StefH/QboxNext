using QboxNext.Core.Dto;
using QboxNext.Core.Utils;
using QboxNext.Extensions.Interfaces.Internal;
using QboxNext.Model.Qboxes;
using QboxNext.Model.Utils;

namespace QboxNext.Extensions.Implementations
{
    internal class QboxMiniFactory : IQboxMiniFactory
    {
        /// <inheritdoc cref="IQboxMiniFactory.Create(string)"/>
        public Mini Create(string serialNumber)
        {
            Guard.IsNotNullOrEmpty(serialNumber, nameof(serialNumber));

            return new QboxBuilder()
                .WithType(QboxType.Duo)
                .WithSerial(serialNumber)
                .WithCounter(QboxConstants.CounterIdConsumptionLow, CounterType.Primary, DeviceEnergyType.NetLow)
                .WithCounter(QboxConstants.CounterIdConsumptionHigh, CounterType.Primary, DeviceEnergyType.NetHigh)
                .WithCounter(QboxConstants.CounterIdGenerationLow, CounterType.Primary, DeviceEnergyType.NetLow)
                .WithCounter(QboxConstants.CounterIdGenerationHigh, CounterType.Primary, DeviceEnergyType.NetHigh)
                .WithCounter(QboxConstants.CounterIdGasConsumption, CounterType.Primary, DeviceEnergyType.Gas)
                .WithCounter(1, CounterType.Secondary, DeviceEnergyType.Generation)
                .Build();
        }
    }
}