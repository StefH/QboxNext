using System.Collections;

namespace QboxNext.Qboxes.Parsing.Elements
{
    public class PayloaderIndicator
    {
        private BitArray Data { get; set; }
        public PayloaderIndicator(byte b)
        {
            Data = new BitArray(new byte[1] { b });
        }

        public int NrOfCounters
        {
            get
            {
                var result = new BitArray(8, false);
                result[0] = Data[0];
                result[1] = Data[1];
                result[2] = Data[2];
                result[3] = Data[3];

                return BitArrayUtility.GetIntFromBitArray(result);
            }
        }

        public bool ClientStatusPresent
        {
            get { return Data[4]; }
        }

        public bool DeviceSettingPresent
        {
            get { return Data[6]; }
        }

        public bool SmartMeterIsPresent
        {
            get { return Data[7]; }
        }
    }
}
