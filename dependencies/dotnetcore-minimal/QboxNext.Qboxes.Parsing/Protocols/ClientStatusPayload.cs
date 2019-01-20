using System;
using System.Collections;
using Microsoft.Extensions.Logging;
using QboxNext.Logging;
using QboxNext.Qboxes.Parsing.Elements;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class ClientStatusPayload : BasePayload
	{
	    private static readonly ILogger Logger = QboxNextLogProvider.CreateLogger<ClientStatusPayload>();

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
			Logger.LogTrace("Enter");
			visitor.Accept(this);
			Logger.LogTrace("Exit");
		}
	}

}
