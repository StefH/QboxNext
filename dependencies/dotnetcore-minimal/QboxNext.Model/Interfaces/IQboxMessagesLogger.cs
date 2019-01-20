using QboxNext.Core.Dto;

namespace QboxNext.Model.Interfaces
{
	/// <summary>
	/// Interface voor het loggen van Qbox berichten.
	/// </summary>
	public interface IQboxMessagesLogger
	{
		void LogQboxMessage(string serial, string message, QboxMessageType messageType);
	}
}
