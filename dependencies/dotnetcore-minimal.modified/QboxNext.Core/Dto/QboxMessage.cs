using System;

namespace QboxNext.Core.Dto
{
	public enum QboxMessageType
	{
		Request,
		Response,
		Error,
		Exception,
		Trace
	}


	public class QboxMessage
	{
		public DateTime TimestampUtc { get; set; }
		public string QboxSerial { get; set; }
		public QboxMessageType MessageType { get; set; }
		public string Message { get; set; }
	}
}
