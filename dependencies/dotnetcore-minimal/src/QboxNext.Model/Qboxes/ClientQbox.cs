using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Model.Qboxes
{
    public class ClientQbox
    {
        public byte ClientId { get; set; }
        public DeviceMeterType MeterType { get; set; }
        public DeviceMeterType SecondaryMeterType { get; set; }
    }
}
