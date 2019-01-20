namespace QboxNext.Qboxes.Parsing.Protocols
{
	public abstract class BasePayload
	{
		public abstract void Visit(IVisitor visitor);
	}
}