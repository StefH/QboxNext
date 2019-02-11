using QboxNext.Core.Dto;

namespace QboxNext.Qservice.Classes
{
    public class LiveDataForDevice
    {
        public void Scale(decimal inFraction)
        {
            Power *= inFraction;
        }


        public string Name { get; set; }
        public DeviceEnergyType EnergyType { get; set; }
        public decimal? Power { get; set; }
    }
}
