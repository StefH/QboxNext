using System;
using System.Collections;
using QboxNext.Qboxes.Parsing.Elements;
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class ClientStatusPayload : BasePayload
	{
		private static readonly ILog Log = LogProvider.GetCurrentClassLogger();

		public ClientMiniStatus State { get; private set; }
		private BitArray Data { get; set; }
		public DateTime MeasurementTime { get; private set; }
		public byte RawValue { get; private set; }
		public byte Client { get; private set; }
		public ClientStatusPayload(DateTime measurementTime, byte client, byte b, int protocolNr)
		{
			MeasurementTime = measurementTime;
			State = new ClientMiniStatus(b, protocolNr);
			Data = new BitArray(new byte[1] { b });
			RawValue = b;
			Client = client;
		}

		public override void Visit(IVisitor visitor)
		{
			Log.Trace("Enter");
			visitor.Accept(this);
			Log.Trace("Exit");
		}
	}

}
