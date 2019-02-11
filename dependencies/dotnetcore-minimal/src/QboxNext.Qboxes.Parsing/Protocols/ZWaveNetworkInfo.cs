using System.Collections.Generic;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class ZWaveNetworkInfo
	{
		public ZWaveNetworkInfo()
		{
			ClientInfos = new List<ZWaveClientInfo>();
			Devices = new List<ZWaveNetworkDevice>();
		}

		public int NumberOfDevices { get; set; }  // 0x47

		public IList<ZWaveClientInfo> ClientInfos { get; private set; }  // part of 0x4D
		public IList<ZWaveNetworkDevice> Devices { get; private set; }  // 0x60
	}
}