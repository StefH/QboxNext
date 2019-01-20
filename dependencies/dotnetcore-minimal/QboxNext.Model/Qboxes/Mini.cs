using QboxNext.Qboxes.Parsing.Protocols;
using System.Collections.Generic;

namespace QboxNext.Model.Qboxes
{
    public class Mini
    {
        public Mini()
        {
            Counters = new List<Counter>();
        }

        public IList<Counter> Counters { get; set; }

        public static string WriteMeterType(DeviceMeterType deviceMeterType)
        {
            var result = new BaseParseResult();
            result.Write((byte)3);
            var meterType = (byte)deviceMeterType;
            result.Write(meterType);
            return result.GetMessage();
        }
    }
}