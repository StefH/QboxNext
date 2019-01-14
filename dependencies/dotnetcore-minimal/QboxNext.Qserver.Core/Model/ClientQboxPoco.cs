using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qserver.Core.Model
{
    public class ClientQboxPoco
    {
        public byte ClientId { get; set; }
        public DeviceMeterType MeterType { get; set; }
        public DeviceMeterType SecondaryMeterType { get; set; }
    }
}
