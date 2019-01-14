namespace QboxNext.Qboxes.Parsing.Protocols
{
	public interface IVisitor
	{
		void Accept(CounterPayload payload);
		void Accept(DeviceSettingsPayload payload);
		void Accept(ClientStatusPayload payload);
	}
}