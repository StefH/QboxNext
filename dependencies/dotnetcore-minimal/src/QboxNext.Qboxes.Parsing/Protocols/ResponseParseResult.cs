using System;
using System.Collections.Generic;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class ResponseParseResult : BaseParseResult
	{
		public ResponseParseResult()
		{
			DeviceSettings = new List<BasePayload>();
		}
		public DateTime ResponseTime { get; set; }
		public int Offset { get; set; }
		public IList<BasePayload> DeviceSettings { get; private set; }
	}
}