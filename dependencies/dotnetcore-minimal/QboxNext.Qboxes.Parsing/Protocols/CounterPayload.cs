
using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public class CounterPayload : BasePayload
	{
		private static readonly ILog Log = LogProvider.GetCurrentClassLogger();
		public int InternalNr { get; set; }
		public ulong Value { get; set; }
		public override void Visit(IVisitor visitor)
		{
			Log.Trace("Enter");
			visitor.Accept(this);
			Log.Trace("Exit");
		}
	}
}
