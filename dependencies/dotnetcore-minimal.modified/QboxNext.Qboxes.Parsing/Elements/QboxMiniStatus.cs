using System.Collections;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Elements
{
    public class QboxMiniStatus
    {
        private readonly BitArray _p;

        public QboxMiniStatus(byte p)
        {
            var bytes = new byte[1] { p };
            _p = new BitArray(bytes);
        }

        public bool ValidResponse
        {
            get { return _p[0]; }
        }

        public bool TimeIsReliable
        {
            get { return _p[1]; }
        }

        public MiniState Status
        {
            get
            {
                var local = new BitArray(8, false);
                local[0] = _p[2];
                local[1] = _p[3];
                local[2] = _p[4];
                var result = BitArrayUtility.GetIntFromBitArray(local);
                return (MiniState)result;
            }
        }
    }
}
