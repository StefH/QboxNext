using QboxNext.Core.Dto;


namespace Qboxes.Interfaces
{
	/// <summary>
	/// Interface voor het loggen van Qbox berichten.
	/// </summary>
	public interface IQboxMessagesLogger
	{
		void LogQboxMessage(string serial, string message, QboxMessageType messageType);
	}
}
