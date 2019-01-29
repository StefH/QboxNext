using QboxNext.Core.Dto;
using QboxNext.Model.Interfaces;
using QboxNext.Model.Qboxes;
using QboxNext.Model.Utils;

namespace QboxNext.Model.Classes
{
    public class ConfiguredMiniRetriever : IMiniRetriever
    {
        private readonly QboxType _qboxType;

        public ConfiguredMiniRetriever(QboxType qboxType)
        {
            _qboxType = qboxType;
        }

        public Mini Retrieve(string qboxSerial)
        {
            Mini mini = new QboxBuilder()
                .WithType(_qboxType)
                .WithSerial(qboxSerial)
                .WithCounter(181, CounterType.Primary, DeviceEnergyType.Consumption)
                .WithCounter(182, CounterType.Primary, DeviceEnergyType.Consumption)
                .WithCounter(281, CounterType.Primary, DeviceEnergyType.Generation)
                .WithCounter(282, CounterType.Primary, DeviceEnergyType.Generation)
                .WithCounter(2421, CounterType.Primary, DeviceEnergyType.Gas)
                .WithCounter(1, CounterType.Secondary, DeviceEnergyType.Generation)
                .Build();
            mini.PrepareCounters();
            return mini;
        }
    }
}
