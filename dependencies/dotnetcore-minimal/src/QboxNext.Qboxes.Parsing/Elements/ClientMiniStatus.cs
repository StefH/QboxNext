using System.Collections;
using QboxNext.Qboxes.Parsing.Protocols;

namespace QboxNext.Qboxes.Parsing.Elements
{
    public class ClientMiniStatus
    {
        private BitArray Data { get; set; }
		private int ProtocolNr { get; set; }
        public byte RawValue { get; private set; }
		public ClientMiniStatus(byte b, int protocolNr)
        {
            Data = new BitArray(new byte[1] { b });
            RawValue = b;
			ProtocolNr = protocolNr; 
        } 

        public bool ConnectionWithClient
        {
            get { return Data[0]; }
        }

        public bool ActionsFailed
        {
            get { return Data[1]; }
        }

        public ClientState State
        {
            get
            {
                var result = new BitArray(8, false);
                result[0] = Data[2];
                result[1] = Data[3];
                result[2] = Data[4];
				if (ProtocolNr >= MiniR21.ProtocolVersion)  
					result[3] = Data[5];

                return (ClientState)BitArrayUtility.GetIntFromBitArray(result);
            }
        }
    }
}
