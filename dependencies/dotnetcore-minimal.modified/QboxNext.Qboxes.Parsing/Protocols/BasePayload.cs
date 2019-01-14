using QboxNext.Qboxes.Parsing.Logging;

namespace QboxNext.Qboxes.Parsing.Protocols
{
	public abstract class BasePayload
	{
		private static readonly ILog Log = LogProvider.GetLogger("BasePayLoad");
		public abstract void Visit(IVisitor visitor);
	}
}