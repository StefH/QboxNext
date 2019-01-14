using System.Collections.Generic;
using Qboxes.Model.Qboxes;
using QboxNext.Qboxes.Parsing.Protocols;

namespace Qboxes
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

            //Rolf 25-4-2013: In overleg met Ron verwijderd voor dit moment
            ////todo: manufacturer
            //if (deviceMeterType == DeviceMeterType.Ferraris_Black_Toothed)
            //{
            //    result.Write((byte)5);
            //    const short manufacturer = 0x00000101;
            //    result.Write(manufacturer);
            //}

            return result.GetMessage();
        }
    }
}
