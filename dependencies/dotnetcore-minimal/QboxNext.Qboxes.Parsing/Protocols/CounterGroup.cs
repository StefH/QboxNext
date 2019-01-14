using System.Collections;
using QboxNext.Qboxes.Parsing.Elements;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class CounterGroup
	{
		private BitArray Data { get; set; }
		public byte RawValue { get; private set; }
		public CounterGroup(byte b)
		{
			Data = new BitArray(new byte[1] { b });
		}

		public int NbrOfCounters
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

		public CounterSource CounterSource
		{
			get
			{
				var result = new BitArray(8, false);
				result[0] = Data[4];
				result[1] = Data[5];
				result[2] = Data[6];

				return (CounterSource)BitArrayUtility.GetIntFromBitArray(result);
			}
		}

		public bool PrimaryMeterCounters
		{
			get { return Data[7]; }
		}
	}
}