using System;
using System.Collections.Generic;
using QboxNext.Qboxes.Parsing.Elements;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class MiniParseModel
	{
		public MiniParseModel()
		{
			Payloads = new List<BasePayload>();
		}
		public QboxMiniStatus Status { get; set; }
		public DateTime MeasurementTime { get; set; }
		public DeviceMeterType MeterType { get; set; }
		public IList<BasePayload> Payloads { get; private set; }
	}
}