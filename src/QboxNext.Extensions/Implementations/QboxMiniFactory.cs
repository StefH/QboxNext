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
                .WithCounter(181, CounterType.Primary, DeviceEnergyType.NetLow)
                .WithCounter(182, CounterType.Primary, DeviceEnergyType.NetHigh)
                .WithCounter(281, CounterType.Primary, DeviceEnergyType.NetLow)
                .WithCounter(282, CounterType.Primary, DeviceEnergyType.NetHigh)
                .WithCounter(2421, CounterType.Primary, DeviceEnergyType.Gas)
                .Build();
        }
    }
}